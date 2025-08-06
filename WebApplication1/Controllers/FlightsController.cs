using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _flightService.GetAllCitiesAsync();
            var viewModel = new FlightSearchViewModel
            {
                AvailableCities = cities,
                DepartureDate = DateTime.Today.AddDays(1)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(FlightSearchViewModel model)
        {
            model.AvailableCities = await _flightService.GetAllCitiesAsync();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Validate departure date is not in the past
            if (model.DepartureDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("DepartureDate", "Departure date cannot be in the past");
                return View("Index", model);
            }

            // Validate departure date is within booking window (3 days to 6 months)
            var minDate = DateTime.Today.AddDays(3);
            var maxDate = DateTime.Today.AddMonths(6);
            
            if (model.DepartureDate.Date < minDate || model.DepartureDate.Date > maxDate)
            {
                ModelState.AddModelError("DepartureDate", $"Flights can only be booked between {minDate:yyyy-MM-dd} and {maxDate:yyyy-MM-dd}");
                return View("Index", model);
            }

            // Validate return date if provided
            if (model.ReturnDate.HasValue && model.ReturnDate.Value.Date <= model.DepartureDate.Date)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after departure date");
                return View("Index", model);
            }

            // Validate origin and destination are different
            if (model.OriginCityId == model.DestinationCityId)
            {
                ModelState.AddModelError("DestinationCityId", "Destination must be different from origin");
                return View("Index", model);
            }

            try
            {
                // Get city details for airport codes
                var cities = await _flightService.GetAllCitiesAsync();
                var originCity = cities.FirstOrDefault(c => c.Id == model.OriginCityId);
                var destinationCity = cities.FirstOrDefault(c => c.Id == model.DestinationCityId);

                if (originCity == null || destinationCity == null)
                {
                    ModelState.AddModelError("", "Selected cities are not valid");
                    return View("Index", model);
                }

                // Create search request with enhanced parameters
                var searchRequest = new FlightSearchRequest
                {
                    OriginLocationCode = originCity.AirportCode,
                    DestinationLocationCode = destinationCity.AirportCode,
                    DepartureDate = model.DepartureDate,
                    ReturnDate = model.ReturnDate,
                    Adults = model.Passengers,
                    TravelClass = model.TravelClass == FlightClass.Economy ? "ECONOMY" : "BUSINESS",
                    NonStop = model.NonStopOnly,
                    Max = 10,
                    CurrencyCode = "USD",
                    IncludedCheckedBagsOnly = false,
                    AddOneWayOffers = model.ReturnDate == null
                };

                // Search for flights
                var flights = await _flightService.SearchFlightsAsync(searchRequest);
                
                model.SearchResults = flights;
                model.HasSearched = true;

                _logger.LogInformation("Flight search completed. Found {Count} flights from {Origin} to {Destination} on {Date}", 
                    flights.Count, originCity.Name, destinationCity.Name, model.DepartureDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during flight search");
                ModelState.AddModelError("", "An error occurred while searching for flights. Please try again.");
            }

            return View("Index", model);
        }

        public IActionResult Details(int id)
        {
            // In a real application, you'd fetch the flight from database
            // For now, let's create a simple detail view
            return View();
        }

        public IActionResult Book(int flightId, int passengers = 1, int travelClass = 1)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Flights", new { flightId, passengers, travelClass }) });
            }

            // Redirect to booking page with passenger count and travel class
            // Ensure travel class parameter is properly passed
            return RedirectToAction("Create", "Booking", new { flightId, passengers, travelClass });
        }
    }
}