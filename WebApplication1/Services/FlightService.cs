using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.DTOs;
using System.Text.Json;

namespace WebApplication1.Services
{
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmadeusService _amadeusService;
        private readonly ISeatService _seatService;
        private readonly ILogger<FlightService> _logger;

        public FlightService(ApplicationDbContext context, IAmadeusService amadeusService, ISeatService seatService, ILogger<FlightService> logger)
        {
            _context = context;
            _amadeusService = amadeusService;
            _seatService = seatService;
            _logger = logger;
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Models.Route>> GetAvailableRoutesAsync(int originCityId, int destinationCityId)
        {
            return await _context.Routes
                .Include(r => r.OriginCity)
                .Include(r => r.DestinationCity)
                .Include(r => r.TransferCity)
                .Where(r => r.OriginCityId == originCityId && r.DestinationCityId == destinationCityId && r.IsActive)
                .ToListAsync();
        }

        public async Task<List<Flight>> SearchFlightsAsync(FlightSearchRequest request)
        {
            try
            {
                // Get cities from our database to map airport codes
                var originCity = await _context.Cities
                    .FirstOrDefaultAsync(c => c.AirportCode == request.OriginLocationCode);
                var destinationCity = await _context.Cities
                    .FirstOrDefaultAsync(c => c.AirportCode == request.DestinationLocationCode);

                if (originCity == null || destinationCity == null)
                {
                    _logger.LogWarning("Could not find cities for airport codes: {Origin}, {Destination}", 
                        request.OriginLocationCode, request.DestinationLocationCode);
                    return new List<Flight>();
                }

                // Convert to UTC for PostgreSQL compatibility
                var searchDate = DateTime.SpecifyKind(request.DepartureDate.Date, DateTimeKind.Utc);
                
                // Search for existing flights in our database first
                var existingFlights = await _context.Flights
                    .Include(f => f.OriginCity)
                    .Include(f => f.DestinationCity)
                    .Where(f => f.OriginCityId == originCity.Id && 
                               f.DestinationCityId == destinationCity.Id &&
                               f.DepartureDate.Date == searchDate.Date &&
                               f.IsActive)
                    .ToListAsync();

                // If we have existing flights, return them with seasonal pricing applied
                if (existingFlights.Any())
                {
                    foreach (var flight in existingFlights)
                    {
                        flight.EconomyPrice = await CalculateSeasonalPriceAsync(flight.EconomyPrice, originCity.Id, destinationCity.Id, request.DepartureDate);
                        flight.BusinessPrice = await CalculateSeasonalPriceAsync(flight.BusinessPrice, originCity.Id, destinationCity.Id, request.DepartureDate);
                        
                        // Ensure seats are generated for existing flights
                        await _seatService.GenerateSeatsForFlightAsync(flight.Id);
                        
                        // Check if seats need simulation (only if no seats are already occupied)
                        var existingOccupiedSeats = await _context.Seats
                            .Where(s => s.FlightId == flight.Id && !s.IsAvailable)
                            .CountAsync();
                            
                        if (existingOccupiedSeats == 0)
                        {
                            // Simulate some occupied seats for realistic seat maps
                            var random = new Random();
                            var economyOccupied = random.Next(15, 45); // 15-45 economy seats occupied (out of 132)
                            var businessOccupied = random.Next(2, 8);  // 2-8 business seats occupied (out of 18)
                            
                            await _seatService.SimulateOccupiedSeatsAsync(flight.Id, economyOccupied, businessOccupied);
                        }
                    }
                    return existingFlights;
                }

                // If no existing flights, try to get from Amadeus API and create synthetic flights
                var amadeusResponse = await _amadeusService.SearchFlightsAsync(request);
                
                if (amadeusResponse?.Data?.Any() == true)
                {
                    return await CreateAndPersistSyntheticFlightsFromAmadeusAsync(amadeusResponse, originCity, destinationCity, request.DepartureDate);
                }

                // Fallback: Create default daily flights as per requirements
                return await CreateAndPersistDefaultFlightsAsync(originCity, destinationCity, request.DepartureDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights for {Origin} to {Destination} on {Date}", 
                    request.OriginLocationCode, request.DestinationLocationCode, request.DepartureDate);
                
                // Return empty list in case of error
                return new List<Flight>();
            }
        }

        public async Task<decimal> CalculateSeasonalPriceAsync(decimal basePrice, int originCityId, int destinationCityId, DateTime departureDate)
        {
            var originCity = await _context.Cities.FindAsync(originCityId);
            var destinationCity = await _context.Cities.FindAsync(destinationCityId);

            if (originCity == null || destinationCity == null)
                return Math.Round(basePrice, 2);

            var month = departureDate.Month;

            // Christmas season (December 1-31) - 30% increase for London and New York routes
            if (month == 12)
            {
                if (originCity.Name.Contains("London") || originCity.Name.Contains("New York") ||
                    destinationCity.Name.Contains("London") || destinationCity.Name.Contains("New York"))
                {
                    return Math.Round(basePrice * 1.30m, 2);
                }
            }

            // Summer vacation season (July-August) - 30% increase for Tokyo, Singapore, Dubai routes
            if (month == 7 || month == 8)
            {
                if (originCity.Name.Contains("Tokyo") || originCity.Name.Contains("Singapore") || originCity.Name.Contains("Dubai") ||
                    destinationCity.Name.Contains("Tokyo") || destinationCity.Name.Contains("Singapore") || destinationCity.Name.Contains("Dubai"))
                {
                    return Math.Round(basePrice * 1.30m, 2);
                }
            }

            return Math.Round(basePrice, 2);
        }

        public async Task<List<Meal>> GetAvailableMealsAsync()
        {
            return await _context.Meals
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Type)
                .ToListAsync();
        }

        private async Task<List<Flight>> CreateAndPersistSyntheticFlightsFromAmadeusAsync(FlightOffersSearchResponse amadeusResponse, City originCity, City destinationCity, DateTime departureDate)
        {
            var flights = await CreateSyntheticFlightsFromAmadeusAsync(amadeusResponse, originCity, destinationCity, departureDate);
            
            if (flights.Any())
            {
                // Save flights to database
                _context.Flights.AddRange(flights);
                await _context.SaveChangesAsync();
                
                // Generate seats for each flight
                foreach (var flight in flights)
                {
                    await _seatService.GenerateSeatsForFlightAsync(flight.Id);
                    
                    // Simulate some occupied seats for realistic seat maps
                    var random = new Random();
                    var economyOccupied = random.Next(15, 45); // 15-45 economy seats occupied (out of 132)
                    var businessOccupied = random.Next(2, 8);  // 2-8 business seats occupied (out of 18)
                    
                    await _seatService.SimulateOccupiedSeatsAsync(flight.Id, economyOccupied, businessOccupied);
                }
            }
            
            return flights;
        }

        private async Task<List<Flight>> CreateAndPersistDefaultFlightsAsync(City originCity, City destinationCity, DateTime departureDate)
        {
            var flights = await CreateDefaultFlightsAsync(originCity, destinationCity, departureDate);
            
            if (flights.Any())
            {
                // Save flights to database
                _context.Flights.AddRange(flights);
                await _context.SaveChangesAsync();
                
                // Generate seats for each flight
                foreach (var flight in flights)
                {
                    await _seatService.GenerateSeatsForFlightAsync(flight.Id);
                    
                    // Simulate some occupied seats for realistic seat maps
                    var random = new Random();
                    var economyOccupied = random.Next(15, 45); // 15-45 economy seats occupied (out of 132)
                    var businessOccupied = random.Next(2, 8);  // 2-8 business seats occupied (out of 18)
                    
                    await _seatService.SimulateOccupiedSeatsAsync(flight.Id, economyOccupied, businessOccupied);
                }
            }
            
            return flights;
        }

        private async Task<List<Flight>> CreateSyntheticFlightsFromAmadeusAsync(FlightOffersSearchResponse amadeusResponse, City originCity, City destinationCity, DateTime departureDate)
        {
            var flights = new List<Flight>();
            var seenFlights = new HashSet<string>(); // To avoid duplicates
            
            foreach (var offer in amadeusResponse.Data.Take(5)) // Limit to 5 flights
            {
                if (offer.Itineraries?.Any() == true)
                {
                    var itinerary = offer.Itineraries.First();
                    var firstSegment = itinerary.Segments?.FirstOrDefault();
                    var lastSegment = itinerary.Segments?.LastOrDefault();

                    if (firstSegment != null && lastSegment != null)
                    {
                        // Create unique identifier for deduplication
                        var flightKey = $"{firstSegment.CarrierCode}{firstSegment.Number}_{firstSegment.Departure.At:yyyyMMddHHmm}_{lastSegment.Arrival.At:yyyyMMddHHmm}";
                        
                        if (seenFlights.Contains(flightKey))
                            continue;
                        
                        seenFlights.Add(flightKey);

                        // Parse price properly - handle both "232.86" format and potential decimal places
                        if (!decimal.TryParse(offer.Price.Total, System.Globalization.NumberStyles.Number, 
                            System.Globalization.CultureInfo.InvariantCulture, out var totalPrice))
                        {
                            totalPrice = 500m; // Fallback price
                        }

                        // Round to 2 decimal places
                        totalPrice = Math.Round(totalPrice, 2);
                        
                        // Extract stopover information
                        var segments = itinerary.Segments ?? new List<Segment>();
                        var numberOfStops = Math.Max(0, segments.Count - 1);
                        var stopoverInfo = ExtractStopoverInfo(segments, amadeusResponse.Dictionaries);
                        
                        // Create flight number with transfer info if applicable
                        var flightNumber = numberOfStops > 0 
                            ? $"{firstSegment.CarrierCode}{firstSegment.Number}"
                            : $"{firstSegment.CarrierCode}{firstSegment.Number}";
                        
                        var flight = new Flight
                        {
                            FlightNumber = flightNumber,
                            OriginCityId = originCity.Id,
                            DestinationCityId = destinationCity.Id,
                            DepartureDate = DateTime.SpecifyKind(firstSegment.Departure.At, DateTimeKind.Utc),
                            ArrivalDate = DateTime.SpecifyKind(lastSegment.Arrival.At, DateTimeKind.Utc),
                            EconomyPrice = totalPrice,
                            BusinessPrice = Math.Round(totalPrice * 2.5m, 2), // Business is typically 2.5x economy
                            EconomySeats = 150,
                            BusinessSeats = 30,
                            EconomyAvailableSeats = offer.NumberOfBookableSeats,
                            BusinessAvailableSeats = Math.Min(offer.NumberOfBookableSeats / 5, 30),
                            NumberOfStops = numberOfStops,
                            StopoverInfo = stopoverInfo != null ? JsonSerializer.Serialize(stopoverInfo) : null,
                            IsActive = true,
                            OriginCity = originCity,
                            DestinationCity = destinationCity
                        };

                        // Apply seasonal pricing
                        flight.EconomyPrice = await CalculateSeasonalPriceAsync(flight.EconomyPrice, originCity.Id, destinationCity.Id, departureDate);
                        flight.BusinessPrice = await CalculateSeasonalPriceAsync(flight.BusinessPrice, originCity.Id, destinationCity.Id, departureDate);

                        flights.Add(flight);
                    }
                }
            }

            return flights;
        }

        private async Task<List<Flight>> CreateDefaultFlightsAsync(City originCity, City destinationCity, DateTime departureDate)
        {
            // Create at least one daily flight as per requirements
            var basePrice = CalculateBasePrice(originCity, destinationCity);
            
            // Convert to UTC for PostgreSQL compatibility
            var departureUtc = DateTime.SpecifyKind(departureDate.Date.AddHours(10), DateTimeKind.Utc);
            var arrivalUtc = DateTime.SpecifyKind(departureDate.Date.AddHours(14), DateTimeKind.Utc);
            
            var flight = new Flight
            {
                FlightNumber = $"FL{Random.Shared.Next(1000, 9999)}",
                OriginCityId = originCity.Id,
                DestinationCityId = destinationCity.Id,
                DepartureDate = departureUtc, // 10 AM departure
                ArrivalDate = arrivalUtc, // 4 PM arrival (assumes 4-hour flight)
                EconomyPrice = Math.Round(basePrice, 2),
                BusinessPrice = Math.Round(basePrice * 2.5m, 2),
                EconomySeats = 150,
                BusinessSeats = 30,
                EconomyAvailableSeats = 120,
                BusinessAvailableSeats = 25,
                IsActive = true,
                OriginCity = originCity,
                DestinationCity = destinationCity
            };

            // Apply seasonal pricing
            flight.EconomyPrice = await CalculateSeasonalPriceAsync(flight.EconomyPrice, originCity.Id, destinationCity.Id, departureDate);
            flight.BusinessPrice = await CalculateSeasonalPriceAsync(flight.BusinessPrice, originCity.Id, destinationCity.Id, departureDate);

            return new List<Flight> { flight };
        }

        private static decimal CalculateBasePrice(City originCity, City destinationCity)
        {
            // Simple distance-based pricing calculation
            var distance = CalculateDistance(originCity.Latitude, originCity.Longitude, 
                                           destinationCity.Latitude, destinationCity.Longitude);
            
            // Base price calculation: $0.10 per km + $200 base fee
            return Math.Round((decimal)(distance * 0.10 + 200), 2);
        }

        private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        private StopoverInfo? ExtractStopoverInfo(List<Segment> segments, Dictionaries dictionaries)
        {
            if (segments == null || segments.Count <= 1)
                return null;

            var stopovers = new List<Stopover>();

            // Process intermediate segments (stopovers)
            for (int i = 0; i < segments.Count - 1; i++)
            {
                var currentSegment = segments[i];
                var nextSegment = segments[i + 1];

                // The arrival of current segment is a stopover
                var arrivalAirport = currentSegment.Arrival.IataCode;
                var departureTime = nextSegment.Departure.At;
                var layoverDuration = departureTime - currentSegment.Arrival.At;

                // Try to get city name from dictionaries
                var cityName = arrivalAirport; // Default to airport code
                var countryCode = "";

                if (dictionaries?.Locations?.ContainsKey(arrivalAirport) == true)
                {
                    var location = dictionaries.Locations[arrivalAirport];
                    cityName = location.CityCode;
                    countryCode = location.CountryCode;
                }

                var stopover = new Stopover
                {
                    AirportCode = arrivalAirport,
                    CityName = GetCityNameFromCode(cityName), // Convert airport code to readable name
                    CountryCode = countryCode,
                    ArrivalTime = currentSegment.Arrival.At,
                    DepartureTime = departureTime,
                    LayoverDuration = layoverDuration,
                    Terminal = currentSegment.Arrival.Terminal ?? ""
                };

                stopovers.Add(stopover);
            }

            return new StopoverInfo { Stopovers = stopovers };
        }

        private static string GetCityNameFromCode(string code)
        {
            // Map common airport/city codes to readable names
            var cityMappings = new Dictionary<string, string>
            {
                // Major cities in our system
                { "RUH", "Riyadh" },
                { "DXB", "Dubai" },
                { "LHR", "London" },
                { "JFK", "New York" },
                { "NRT", "Tokyo" },
                { "SIN", "Singapore" },
                { "SYD", "Sydney" },
                { "CPT", "Cape Town" },
                
                // Common stopover cities
                { "DOH", "Doha" },
                { "AUH", "Abu Dhabi" },
                { "IST", "Istanbul" },
                { "CDG", "Paris" },
                { "FRA", "Frankfurt" },
                { "AMS", "Amsterdam" },
                { "ZUR", "Zurich" },
                { "ZRH", "Zurich" },
                { "VIE", "Vienna" },
                { "MUC", "Munich" },
                { "FCO", "Rome" },
                { "BCN", "Barcelona" },
                { "MAD", "Madrid" },
                { "LON", "London" },
                
                // North American airports
                { "YTO", "Toronto" },
                { "YYZ", "Toronto" },
                { "YVR", "Vancouver" },
                { "YUL", "Montreal" },
                { "LAX", "Los Angeles" },
                { "SFO", "San Francisco" },
                { "ORD", "Chicago" },
                { "MIA", "Miami" },
                { "ATL", "Atlanta" },
                { "DFW", "Dallas" },
                { "IAH", "Houston" },
                { "SEA", "Seattle" },
                { "BOS", "Boston" },
                { "DEN", "Denver" },
                { "LAS", "Las Vegas" },
                
                // European airports
                { "LGW", "London" },
                { "STN", "London" },
                { "MAN", "Manchester" },
                { "EDI", "Edinburgh" },
                { "DUB", "Dublin" },
                { "ARN", "Stockholm" },
                { "CPH", "Copenhagen" },
                { "OSL", "Oslo" },
                { "HEL", "Helsinki" },
                { "WAW", "Warsaw" },
                { "PRG", "Prague" },
                { "BUD", "Budapest" },
                { "ATH", "Athens" },
                { "LIS", "Lisbon" },
                { "OPO", "Porto" },
                
                // Asian airports
                { "HND", "Tokyo" },
                { "KIX", "Osaka" },
                { "NGO", "Nagoya" },
                { "ICN", "Seoul" },
                { "GMP", "Seoul" },
                { "PEK", "Beijing" },
                { "PVG", "Shanghai" },
                { "CAN", "Guangzhou" },
                { "HKG", "Hong Kong" },
                { "TPE", "Taipei" },
                { "MNL", "Manila" },
                { "BKK", "Bangkok" },
                { "CGK", "Jakarta" },
                { "KUL", "Kuala Lumpur" },
                { "DEL", "New Delhi" },
                { "BOM", "Mumbai" },
                { "BLR", "Bangalore" },
                
                // Middle Eastern airports
                { "BAH", "Bahrain" },
                { "KWI", "Kuwait" },
                { "MCT", "Muscat" },
                { "CAI", "Cairo" },
                { "AMM", "Amman" },
                { "BEY", "Beirut" },
                
                // African airports
                { "JNB", "Johannesburg" },
                { "DUR", "Durban" },
                { "LOS", "Lagos" },
                { "ACC", "Accra" },
                { "NBO", "Nairobi" },
                { "ADD", "Addis Ababa" },
                { "CAS", "Casablanca" },
                { "TUN", "Tunis" },
                { "ALG", "Algiers" },
                
                // Oceania airports
                { "MEL", "Melbourne" },
                { "BNE", "Brisbane" },
                { "PER", "Perth" },
                { "ADL", "Adelaide" },
                { "AKL", "Auckland" },
                { "WLG", "Wellington" },
                { "CHC", "Christchurch" },
                
                // South American airports
                { "GRU", "São Paulo" },
                { "GIG", "Rio de Janeiro" },
                { "EZE", "Buenos Aires" },
                { "SCL", "Santiago" },
                { "LIM", "Lima" },
                { "BOG", "Bogotá" },
                { "UIO", "Quito" },
                { "CCS", "Caracas" }
            };

            return cityMappings.ContainsKey(code.ToUpper()) ? cityMappings[code.ToUpper()] : code;
        }

        public async Task SeedInitialDataAsync()
        {
            if (!await _context.Cities.AnyAsync())
            {
                var cities = new List<City>
                {
                    new City { Name = "New York", CountryCode = "US", AirportCode = "JFK", Latitude = 40.6413m, Longitude = -73.7781m, TimeZone = "America/New_York" },
                    new City { Name = "London", CountryCode = "GB", AirportCode = "LHR", Latitude = 51.4700m, Longitude = -0.4543m, TimeZone = "Europe/London" },
                    new City { Name = "Tokyo", CountryCode = "JP", AirportCode = "NRT", Latitude = 35.7647m, Longitude = 140.3864m, TimeZone = "Asia/Tokyo" },
                    new City { Name = "Dubai", CountryCode = "AE", AirportCode = "DXB", Latitude = 25.2532m, Longitude = 55.3657m, TimeZone = "Asia/Dubai" },
                    new City { Name = "Sydney", CountryCode = "AU", AirportCode = "SYD", Latitude = -33.9399m, Longitude = 151.1753m, TimeZone = "Australia/Sydney" },
                    new City { Name = "Cape Town", CountryCode = "ZA", AirportCode = "CPT", Latitude = -33.9715m, Longitude = 18.6021m, TimeZone = "Africa/Johannesburg" },
                    new City { Name = "Singapore", CountryCode = "SG", AirportCode = "SIN", Latitude = 1.3644m, Longitude = 103.9915m, TimeZone = "Asia/Singapore" }
                };

                _context.Cities.AddRange(cities);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Meals.AnyAsync())
            {
                var meals = new List<Meal>
                {
                    new Meal { Name = "Standard Meal", Description = "Grilled chicken with potatoes and vegetables", Type = MealType.Standard, Price = 0m },
                    new Meal { Name = "Vegetarian Meal", Description = "Vegetable risotto", Type = MealType.Vegetarian, Price = 0m },
                    new Meal { Name = "Vegan Meal", Description = "Lentil curry", Type = MealType.Vegan, Price = 0m },
                    new Meal { Name = "Halal Meal", Description = "Lamb with rice", Type = MealType.Halal, Price = 0m },
                    new Meal { Name = "Kosher Meal", Description = "Appetizer, main course and dessert according to Jewish rules", Type = MealType.Kosher, Price = 0m },
                    new Meal { Name = "Gluten-Free Meal", Description = "Chicken breast with vegetables", Type = MealType.GlutenFree, Price = 0m },
                    new Meal { Name = "Local Inspired Meal", Description = "Pasta Bolognese (destination-specific)", Type = MealType.LocalInspired, Price = 0m }
                };

                _context.Meals.AddRange(meals);
                await _context.SaveChangesAsync();
            }
        }
    }
}