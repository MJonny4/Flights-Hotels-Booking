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
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = DateTime.SpecifyKind(model.DateOfBirth, DateTimeKind.Utc),
                    PassportNumber = model.PassportNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    
                    // Send welcome email
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);
                        _logger.LogInformation("Welcome email sent to {Email}", user.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send welcome email to {Email}", user.Email);
                        // Don't fail registration if email fails
                    }
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    
                    // Update last login time
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var bookingStats = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .GroupBy(b => 1)
                .Select(g => new
                {
                    TotalBookings = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .FirstOrDefaultAsync();

            var viewModel = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                DateOfBirth = user.DateOfBirth,
                PassportNumber = user.PassportNumber,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                TotalBookings = bookingStats?.TotalBookings ?? 0,
                TotalSpent = bookingStats?.TotalSpent ?? 0
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DateOfBirth = model.DateOfBirth;
            user.PassportNumber = model.PassportNumber;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BookingHistory(int page = 1, string? search = null, BookingStatus? status = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            const int pageSize = 10;
            var query = _context.Bookings
                .Where(b => b.UserId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.BookingReference.Contains(search) ||
                                       b.PassengerFirstName.Contains(search) ||
                                       b.PassengerLastName.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            var totalBookings = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBookings / (double)pageSize);

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingHistoryItem
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    BookingDate = b.BookingDate,
                    PassengerName = $"{b.PassengerFirstName} {b.PassengerLastName}",
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    FlightInfo = $"Flight {b.Id}" // This would be replaced with actual flight info
                })
                .ToListAsync();

            var totalSpent = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .SumAsync(b => b.TotalAmount);

            var viewModel = new BookingHistoryViewModel
            {
                Bookings = bookings,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalBookings = totalBookings,
                TotalSpent = totalSpent,
                SearchTerm = search,
                StatusFilter = status
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("BookingHistory");
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "Only pending bookings can be cancelled.";
                return RedirectToAction("BookingHistory");
            }

            if (booking.BookingDate.AddDays(-7) <= DateTime.UtcNow)
            {
                TempData["Error"] = "Bookings can only be cancelled up to 7 days before the booking date.";
                return RedirectToAction("BookingHistory");
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Send cancellation email
            try
            {
                await _emailService.SendBookingCancellationAsync(booking);
                _logger.LogInformation("Cancellation email sent for booking {BookingReference}", booking.BookingReference);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send cancellation email for booking {BookingReference}", booking.BookingReference);
            }

            TempData["Success"] = $"Booking {booking.BookingReference} has been cancelled successfully.";
            return RedirectToAction("BookingHistory");
        }
    }
}