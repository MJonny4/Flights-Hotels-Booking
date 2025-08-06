using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFlightService _flightService;
        private readonly ISeatService _seatService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFlightService flightService,
            ISeatService seatService,
            IEmailService emailService,
            ILogger<BookingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _flightService = flightService;
            _seatService = seatService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int flightId, int passengers = 1, int travelClass = 1)
        {
            // Get flight details
            var flight = await _context.Flights
                .Include(f => f.OriginCity)
                .Include(f => f.DestinationCity)
                .FirstOrDefaultAsync(f => f.Id == flightId);

            if (flight == null)
            {
                return NotFound("Flight not found");
            }

            var selectedClass = (FlightClass)travelClass;
            
            // Check if there are enough seats available
            var hasEnoughSeats = await _seatService.HasEnoughSeatsAsync(flightId, selectedClass, passengers);
            if (!hasEnoughSeats)
            {
                var availableSeats = await _seatService.GetAvailableSeatCountAsync(flightId, selectedClass);
                var className = selectedClass == FlightClass.Economy ? "Economy" : "Business";
                TempData["SeatError"] = $"Not enough {className.ToLower()} class seats available. Only {availableSeats} seats remaining, but you need {passengers} seats.";
                return RedirectToAction("Index", "Flights");
            }

            var viewModel = new BookingViewModel
            {
                FlightId = flightId,
                Flight = flight,
                PassengerCount = passengers,
                SelectedClass = selectedClass,
                AvailableMeals = await _flightService.GetAvailableMealsAsync(),
                AvailableSeats = await GetSeatDtosAsync(flightId, selectedClass)
            };

            // Pre-fill with user data if available
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                viewModel.PassengerFirstName = user.FirstName;
                viewModel.PassengerLastName = user.LastName;
                viewModel.PassportNumber = user.PassportNumber;
                viewModel.PassengerDateOfBirth = user.DateOfBirth;
                viewModel.ContactEmail = user.Email ?? "";
                
                // Use updated phone number from user profile if available, otherwise leave empty
                viewModel.ContactPhone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get flight details
                var flight = await _context.Flights
                    .FirstOrDefaultAsync(f => f.Id == model.FlightId);
                
                if (flight == null)
                {
                    ModelState.AddModelError("", "Flight not found.");
                    model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                    model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                    return View(model);
                }

                // Validate seat selections if provided
                if (model.SelectedSeatIds.Any())
                {
                    // Check if the number of selected seats matches passenger count
                    if (model.SelectedSeatIds.Count != model.PassengerCount)
                    {
                        ModelState.AddModelError("", $"Please select exactly {model.PassengerCount} seat(s) for your {model.PassengerCount} passenger(s).");
                        model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                        model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                        return View(model);
                    }

                    // Check if all selected seats are still available
                    foreach (var seatId in model.SelectedSeatIds)
                    {
                        var isAvailable = await _seatService.IsSeatAvailableAsync(seatId);
                        if (!isAvailable)
                        {
                            ModelState.AddModelError("", "One or more selected seats are no longer available. Please select different seats.");
                            model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                            model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                            return View(model);
                        }
                    }
                }
                else if (model.PassengerCount > 1)
                {
                    ModelState.AddModelError("", $"Please select {model.PassengerCount} seats for your passengers.");
                    model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                    model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                    return View(model);
                }

                // Create booking
                var booking = new Booking
                {
                    BookingReference = GenerateBookingReference(),
                    UserId = user.Id,
                    BookingDate = DateTime.UtcNow,
                    PassengerFirstName = model.PassengerFirstName,
                    PassengerLastName = model.PassengerLastName,
                    PassportNumber = model.PassportNumber,
                    PassengerDateOfBirth = DateTime.SpecifyKind(model.PassengerDateOfBirth, DateTimeKind.Utc),
                    ContactEmail = model.ContactEmail,
                    ContactPhone = model.ContactPhone,
                    Status = BookingStatus.Pending
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Calculate price based on flight and class
                var basePrice = model.SelectedClass == FlightClass.Economy ? flight.EconomyPrice : flight.BusinessPrice;
                var totalPrice = basePrice * model.PassengerCount;
                
                // Create booking flight
                var bookingFlight = new BookingFlight
                {
                    BookingId = booking.Id,
                    FlightId = flight.Id,
                    SeatId = model.PreferredSeatId,
                    MealId = model.IncludeMeal ? model.PreferredMealId : null,
                    FlightClass = model.SelectedClass,
                    Price = totalPrice,
                    HasMeal = model.IncludeMeal
                };

                _context.BookingFlights.Add(bookingFlight);
                
                // Reserve the selected seats
                if (model.SelectedSeatIds.Any())
                {
                    await _seatService.ReserveSeatsAsync(model.SelectedSeatIds);
                }

                booking.TotalAmount = totalPrice;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking created successfully. Reference: {BookingReference}", booking.BookingReference);

                // Send confirmation email
                try
                {
                    await _emailService.SendBookingConfirmationAsync(booking);
                    _logger.LogInformation("Confirmation email sent for booking {BookingReference}", booking.BookingReference);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send confirmation email for booking {BookingReference}", booking.BookingReference);
                    // Don't fail the entire booking if email fails
                }

                return RedirectToAction("Confirmation", new { bookingReference = booking.BookingReference });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                ModelState.AddModelError("", "An error occurred while creating your booking. Please try again.");
                model.AvailableMeals = await _flightService.GetAvailableMealsAsync();
                model.AvailableSeats = await GetSeatDtosAsync(model.FlightId, model.SelectedClass);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(string bookingReference)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);

            if (booking == null)
            {
                return NotFound();
            }

            // Ensure user can only see their own bookings
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || booking.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSeats(int flightId, int flightClass)
        {
            // Get ALL seats for the class to show complete seat map
            var allSeats = await _context.Seats
                .Where(s => s.FlightId == flightId && s.SeatClass == (FlightClass)flightClass)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
                
            return Json(allSeats.Select(s => new { 
                id = s.Id, 
                seatNumber = s.SeatNumber, 
                isWindowSeat = s.IsWindowSeat, 
                isAisleSeat = s.IsAisleSeat,
                isAvailable = s.IsAvailable
            }));
        }

        private async Task<List<WebApplication1.DTOs.SeatDto>> GetSeatDtosAsync(int flightId, FlightClass flightClass)
        {
            // Get ALL seats for the class (both available and occupied) for visual seat map
            var allSeats = await _context.Seats
                .Where(s => s.FlightId == flightId && s.SeatClass == flightClass)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
                
            return allSeats.Select(s => new WebApplication1.DTOs.SeatDto
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                IsWindowSeat = s.IsWindowSeat,
                IsAisleSeat = s.IsAisleSeat,
                IsAvailable = s.IsAvailable
            }).ToList();
        }

        private static string GenerateBookingReference()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}