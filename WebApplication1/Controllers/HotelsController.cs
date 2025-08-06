using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using WebApplication1.DTOs;
using WebApplication1.ViewModels;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class HotelsController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelService hotelService, ApplicationDbContext context, ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get cities for the search dropdown
            var cities = await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new HotelSearchViewModel
            {
                AvailableCities = cities,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                Adults = 1,
                Rooms = 1
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(HotelSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCities = await _context.Cities
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View("Index", model);
            }

            try
            {
                // Get the selected city details
                var selectedCity = await _context.Cities.FindAsync(model.SelectedCityId);
                if (selectedCity == null)
                {
                    ModelState.AddModelError("", "Selected city not found.");
                    model.AvailableCities = await _context.Cities
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name)
                        .ToListAsync();
                    return View("Index", model);
                }

                // Create enhanced hotel search request
                var searchRequest = new HotelSearchRequest
                {
                    CityCode = selectedCity.AirportCode,
                    CheckInDate = model.CheckInDate.ToString("yyyy-MM-dd"),
                    CheckOutDate = model.CheckOutDate.ToString("yyyy-MM-dd"),
                    Adults = model.Adults,
                    Rooms = model.Rooms,
                    CurrencyCode = "USD",
                    BestRateOnly = true,
                    IncludeClosed = false
                };

                // Search for hotels
                var allHotels = await _hotelService.SearchHotelsForDestinationAsync(searchRequest);
                
                // Show only first 3 hotels initially
                const int initialPageSize = 3;
                var displayedHotels = allHotels.Take(initialPageSize).ToList();
                
                // Create result view model
                var resultViewModel = new HotelSearchResultViewModel
                {
                    SearchRequest = model,
                    Hotels = displayedHotels,
                    TotalHotels = allHotels.Count,
                    DisplayedCount = displayedHotels.Count,
                    HasMore = allHotels.Count > initialPageSize,
                    DestinationCity = selectedCity.Name,
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    Adults = model.Adults,
                    Rooms = model.Rooms
                };

                // Store all hotels in TempData for pagination
                TempData["AllHotels"] = System.Text.Json.JsonSerializer.Serialize(allHotels);
                TempData["SearchRequest"] = System.Text.Json.JsonSerializer.Serialize(searchRequest);

                _logger.LogInformation("Hotel search completed. Found {HotelCount} hotels for {City}, showing first {DisplayedCount}", 
                    allHotels.Count, selectedCity.Name, displayedHotels.Count);

                return View("SearchResults", resultViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during hotel search");
                ModelState.AddModelError("", "An error occurred while searching for hotels. Please try again.");
                
                model.AvailableCities = await _context.Cities
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult LoadMoreHotels(int skip = 0, int take = 3)
        {
            try
            {
                // Get hotels from TempData
                var allHotelsJson = TempData["AllHotels"]?.ToString();
                var searchRequestJson = TempData["SearchRequest"]?.ToString();
                
                if (string.IsNullOrEmpty(allHotelsJson) || string.IsNullOrEmpty(searchRequestJson))
                {
                    return Json(new { success = false, message = "Search session expired. Please search again." });
                }

                // Deserialize hotels
                var allHotels = System.Text.Json.JsonSerializer.Deserialize<List<Hotel>>(allHotelsJson);
                var searchRequest = System.Text.Json.JsonSerializer.Deserialize<HotelSearchRequest>(searchRequestJson);

                if (allHotels == null || searchRequest == null)
                {
                    return Json(new { success = false, message = "Invalid search data." });
                }

                // Get the next batch of hotels
                var moreHotels = allHotels.Skip(skip).Take(take).ToList();
                var hasMore = allHotels.Count > (skip + take);

                // Keep data in TempData for next request
                TempData.Keep("AllHotels");
                TempData.Keep("SearchRequest");

                return Json(new
                {
                    success = true,
                    hotels = moreHotels.Select(hotel => new
                    {
                        hotelId = hotel.HotelId,
                        name = hotel.Name,
                        chainCode = hotel.ChainCode,
                        address = new
                        {
                            lines = hotel.Address.Lines,
                            cityName = hotel.Address.CityName,
                            countryCode = hotel.Address.CountryCode
                        },
                        distance = hotel.Distance
                    }),
                    hasMore = hasMore,
                    searchRequest = new
                    {
                        checkInDate = searchRequest.CheckInDate,
                        checkOutDate = searchRequest.CheckOutDate,
                        adults = searchRequest.Adults,
                        rooms = searchRequest.Rooms
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading more hotels");
                return Json(new { success = false, message = "An error occurred while loading more hotels." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string hotelId, string checkInDate, string checkOutDate, int adults = 1, int rooms = 1)
        {
            if (string.IsNullOrEmpty(hotelId))
            {
                _logger.LogWarning("Hotel details requested with empty hotelId");
                return NotFound("Hotel ID is required");
            }

            // Validate date parameters
            if (string.IsNullOrEmpty(checkInDate) || string.IsNullOrEmpty(checkOutDate))
            {
                _logger.LogWarning("Hotel details requested with invalid dates. HotelId: {HotelId}", hotelId);
                return BadRequest("Check-in and check-out dates are required");
            }

            DateTime parsedCheckIn, parsedCheckOut;
            if (!DateTime.TryParse(checkInDate, out parsedCheckIn) || !DateTime.TryParse(checkOutDate, out parsedCheckOut))
            {
                _logger.LogWarning("Hotel details requested with unparseable dates. HotelId: {HotelId}, CheckIn: {CheckIn}, CheckOut: {CheckOut}", 
                    hotelId, checkInDate, checkOutDate);
                return BadRequest("Invalid date format");
            }

            if (parsedCheckIn >= parsedCheckOut || parsedCheckIn < DateTime.Today)
            {
                _logger.LogWarning("Hotel details requested with invalid date range. HotelId: {HotelId}, CheckIn: {CheckIn}, CheckOut: {CheckOut}", 
                    hotelId, checkInDate, checkOutDate);
                return BadRequest("Invalid date range");
            }

            try
            {
                // Get hotel offers for this specific hotel with enhanced parameters
                var offers = await _hotelService.GetHotelOffersAsync(
                    new List<string> { hotelId }, 
                    checkInDate, 
                    checkOutDate, 
                    adults, 
                    rooms,
                    "USD");

                var hotelOffer = offers?.FirstOrDefault();
                if (hotelOffer == null)
                {
                    _logger.LogInformation("No hotel offers found for {HotelId}, attempting to get basic hotel info", hotelId);
                    
                    // If no offers, try to get basic hotel info by searching
                    try
                    {
                        var hotels = await _hotelService.SearchHotelsByKeywordAsync(hotelId);
                        var hotel = hotels?.FirstOrDefault(h => h.HotelId == hotelId);
                        
                        if (hotel == null)
                        {
                            _logger.LogWarning("Hotel not found in search results. HotelId: {HotelId}", hotelId);
                            return NotFound($"Hotel with ID '{hotelId}' not found");
                        }

                        // Create a basic hotel details view without offers
                        var basicViewModel = new HotelDetailsViewModel
                        {
                            HotelId = hotel.HotelId,
                            HotelName = hotel.Name ?? "Hotel Name Not Available",
                            Address = hotel.Address,
                            CheckInDate = parsedCheckIn,
                            CheckOutDate = parsedCheckOut,
                            Adults = adults,
                            Rooms = rooms,
                            HasOffers = false,
                            ChainCode = hotel.ChainCode ?? string.Empty
                        };

                        return View(basicViewModel);
                    }
                    catch (Exception searchEx)
                    {
                        _logger.LogError(searchEx, "Error occurred while searching for basic hotel info for {HotelId}", hotelId);
                        
                        // Create a minimal fallback view model
                        var fallbackViewModel = new HotelDetailsViewModel
                        {
                            HotelId = hotelId,
                            HotelName = "Hotel Information Unavailable",
                            CheckInDate = parsedCheckIn,
                            CheckOutDate = parsedCheckOut,
                            Adults = adults,
                            Rooms = rooms,
                            HasOffers = false
                        };

                        ViewData["ErrorMessage"] = "Unable to load complete hotel information. Please try again later.";
                        return View(fallbackViewModel);
                    }
                }

                // Get hotel sentiments/ratings with error handling
                HotelSentiment? sentiment = null;
                try
                {
                    var sentiments = await _hotelService.GetHotelSentimentsAsync(new List<string> { hotelId });
                    sentiment = sentiments?.FirstOrDefault();
                }
                catch (Exception sentimentEx)
                {
                    _logger.LogWarning(sentimentEx, "Failed to load sentiment data for hotel {HotelId}", hotelId);
                    // Continue without sentiment data
                }

                var viewModel = new HotelDetailsViewModel
                {
                    HotelId = hotelOffer.Hotel?.HotelId ?? hotelId,
                    HotelName = hotelOffer.Hotel?.Name ?? "Hotel Name Not Available",
                    ChainCode = hotelOffer.Hotel?.ChainCode ?? string.Empty,
                    Offers = hotelOffer.Offers ?? new List<Offer>(),
                    CheckInDate = parsedCheckIn,
                    CheckOutDate = parsedCheckOut,
                    Adults = adults,
                    Rooms = rooms,
                    HasOffers = hotelOffer.Offers?.Any() == true,
                    Sentiment = sentiment
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting hotel details for {HotelId}", hotelId);
                
                // Create a minimal error view model to prevent complete crash
                var errorViewModel = new HotelDetailsViewModel
                {
                    HotelId = hotelId,
                    HotelName = "Hotel Details Unavailable",
                    CheckInDate = parsedCheckIn,
                    CheckOutDate = parsedCheckOut,
                    Adults = adults,
                    Rooms = rooms,
                    HasOffers = false
                };

                ViewData["ErrorMessage"] = "We're experiencing technical difficulties loading hotel details. Please try again later.";
                return View(errorViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOffers(string hotelIds, string checkInDate, string checkOutDate, int adults = 1, int rooms = 1)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(hotelIds) || string.IsNullOrEmpty(checkInDate) || string.IsNullOrEmpty(checkOutDate))
                {
                    _logger.LogWarning("GetOffers called with missing parameters");
                    return Json(new List<object>());
                }

                var hotelIdList = hotelIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!hotelIdList.Any())
                {
                    _logger.LogWarning("GetOffers called with empty hotel ID list");
                    return Json(new List<object>());
                }

                // Limit the number of hotels to avoid overwhelming the API
                const int maxHotelsPerRequest = 3; // Further reduced for better performance
                var limitedHotelIds = hotelIdList.Take(maxHotelsPerRequest).ToList();

                if (hotelIdList.Count > maxHotelsPerRequest)
                {
                    _logger.LogInformation("Limited hotel offers request from {OriginalCount} to {LimitedCount} hotels", 
                        hotelIdList.Count, limitedHotelIds.Count);
                }

                var offers = await _hotelService.GetHotelOffersAsync(limitedHotelIds, checkInDate, checkOutDate, adults, rooms, "USD");
                
                var result = offers.Where(ho => ho.Offers?.Any() == true)
                    .SelectMany(ho => ho.Offers.Select(o => new
                    {
                        id = o.Id,
                        hotelId = ho.Hotel?.HotelId ?? "",
                        hotelName = ho.Hotel?.Name ?? "Unknown Hotel",
                        checkInDate = o.CheckInDate,
                        checkOutDate = o.CheckOutDate,
                        roomType = o.Room?.Type ?? "Standard Room",
                        roomDescription = o.Room?.Description?.Text ?? "Room description not available",
                        price = new
                        {
                            currency = o.Price?.Currency ?? "USD",
                            total = o.Price?.Total ?? "0.00",
                            basePrice = o.Price?.Base ?? o.Price?.Total ?? "0.00"
                        }
                    }))
                    .ToList();

                _logger.LogInformation("Successfully loaded {OfferCount} offers for {HotelCount} hotels", result.Count, limitedHotelIds.Count);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting hotel offers for hotels: {HotelIds}", hotelIds);
                // Return empty array instead of error to prevent frontend crashes
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Book(string offerId, string hotelId, string checkInDate, string checkOutDate, int adults = 1, int rooms = 1)
        {
            if (string.IsNullOrEmpty(offerId) || string.IsNullOrEmpty(hotelId))
            {
                return NotFound();
            }

            try
            {
                // Get hotel offers to find the specific offer
                var offers = await _hotelService.GetHotelOffersAsync(
                    new List<string> { hotelId }, 
                    checkInDate, 
                    checkOutDate, 
                    adults, 
                    rooms,
                    "USD");

                var hotelOffer = offers.FirstOrDefault();
                var selectedOffer = hotelOffer?.Offers.FirstOrDefault(o => o.Id == offerId);

                if (hotelOffer == null || selectedOffer == null)
                {
                    return NotFound("Hotel offer not found");
                }

                var viewModel = new HotelBookingViewModel
                {
                    OfferId = selectedOffer.Id,
                    HotelId = hotelOffer.Hotel.HotelId,
                    HotelName = hotelOffer.Hotel.Name,
                    CheckInDate = DateTime.Parse(checkInDate),
                    CheckOutDate = DateTime.Parse(checkOutDate),
                    Adults = adults,
                    Rooms = rooms,
                    RoomType = selectedOffer.Room.Type,
                    RoomDescription = selectedOffer.Room.Description.Text,
                    TotalPrice = decimal.Parse(selectedOffer.Price.Total),
                    Currency = selectedOffer.Price.Currency
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing hotel booking for offer {OfferId}", offerId);
                return StatusCode(500, "An error occurred while preparing your booking.");
            }
        }

        [HttpPost]
        public IActionResult Book(HotelBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Here you would integrate with a real booking system
                // For now, we'll simulate a successful booking
                
                _logger.LogInformation("Hotel booking submitted for {HotelName} by {GuestName}", 
                    model.HotelName, $"{model.GuestFirstName} {model.GuestLastName}");

                // In a real application, you would:
                // 1. Create a booking record in the database
                // 2. Process payment
                // 3. Send confirmation emails
                // 4. Generate booking reference number

                TempData["BookingSuccess"] = "Your hotel booking has been confirmed!";
                TempData["BookingReference"] = $"HTL{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
                TempData["HotelName"] = model.HotelName;
                TempData["CheckInDate"] = model.CheckInDate.ToString("MMM dd, yyyy");
                TempData["CheckOutDate"] = model.CheckOutDate.ToString("MMM dd, yyyy");
                TempData["TotalPrice"] = $"{model.Currency} {model.TotalPrice:N2}";

                return RedirectToAction("BookingConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing hotel booking");
                ModelState.AddModelError("", "An error occurred while processing your booking. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult BookingConfirmation()
        {
            if (TempData["BookingSuccess"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}