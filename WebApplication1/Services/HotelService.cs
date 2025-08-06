using System.Text.Json;
using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public class HotelService : IHotelService
    {
        private readonly IAmadeusService _amadeusService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HotelService> _logger;

        public HotelService(IAmadeusService amadeusService, HttpClient httpClient, ILogger<HotelService> logger)
        {
            _amadeusService = amadeusService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Hotel>> SearchHotelsByCityAsync(string cityCode)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-city?cityCode={cityCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var hotelResponse = JsonSerializer.Deserialize<HotelsByCityResponse>(content, GetJsonOptions());
                    return hotelResponse?.Data ?? new List<Hotel>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to search hotels by city {CityCode}. Status: {StatusCode}, Error: {Error}", 
                        cityCode, response.StatusCode, errorContent);
                    return new List<Hotel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching hotels by city {CityCode}", cityCode);
                return new List<Hotel>();
            }
        }

        public async Task<List<Hotel>> SearchHotelsByGeoCodeAsync(double latitude, double longitude, int radius = 5, string radiusUnit = "KM", string? chainCodes = null)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var queryParams = new List<string>
                {
                    $"latitude={latitude}",
                    $"longitude={longitude}",
                    $"radius={radius}",
                    $"radiusUnit={radiusUnit}"
                };

                if (!string.IsNullOrEmpty(chainCodes))
                {
                    queryParams.Add($"chainCodes={chainCodes}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-geocode?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var hotelResponse = JsonSerializer.Deserialize<HotelsByCityResponse>(content, GetJsonOptions());
                    return hotelResponse?.Data ?? new List<Hotel>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to search hotels by geocode {Latitude},{Longitude}. Status: {StatusCode}, Error: {Error}", 
                        latitude, longitude, response.StatusCode, errorContent);
                    return new List<Hotel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching hotels by geocode {Latitude},{Longitude}", latitude, longitude);
                return new List<Hotel>();
            }
        }

        public async Task<List<Hotel>> SearchHotelsByKeywordAsync(string keyword, string? subType = null)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var url = $"https://test.api.amadeus.com/v1/reference-data/locations/hotel?keyword={keyword}";
                if (!string.IsNullOrEmpty(subType))
                {
                    url += $"&subType={subType}";
                }

                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // The keyword search returns a different structure, we need to adapt it
                    var searchResponse = JsonSerializer.Deserialize<HotelSearchResponse>(content, GetJsonOptions());
                    
                    // Convert search results to Hotel objects
                    return searchResponse?.Data.Select(sr => new Hotel
                    {
                        HotelId = sr.HotelIds.FirstOrDefault() ?? "",
                        Name = sr.Name,
                        IataCode = sr.IataCode,
                        Address = sr.Address,
                        GeoCode = sr.GeoCode
                    }).ToList() ?? new List<Hotel>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to search hotels by keyword {Keyword}. Status: {StatusCode}, Error: {Error}", 
                        keyword, response.StatusCode, errorContent);
                    return new List<Hotel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching hotels by keyword {Keyword}", keyword);
                return new List<Hotel>();
            }
        }

        public async Task<List<HotelOffer>> GetHotelOffersAsync(List<string> hotelIds, string checkInDate, string checkOutDate, int adults = 1, int rooms = 1, string currency = "USD", string? paymentPolicy = null, string? boardType = null)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Limit to maximum 5 hotels per API call to avoid 400 error
                const int maxHotelsPerRequest = 5;
                var allOffers = new List<HotelOffer>();

                // Process hotels in batches
                for (int i = 0; i < hotelIds.Count; i += maxHotelsPerRequest)
                {
                    var batch = hotelIds.Skip(i).Take(maxHotelsPerRequest).ToList();
                    var hotelIdsParam = string.Join(",", batch);
                    
                    var queryParams = new List<string>
                    {
                        $"hotelIds={hotelIdsParam}",
                        $"adults={adults}",
                        $"checkInDate={checkInDate}",
                        $"checkOutDate={checkOutDate}",
                        $"roomQuantity={rooms}",
                        $"currency={currency}"
                    };

                    if (!string.IsNullOrEmpty(paymentPolicy))
                    {
                        queryParams.Add($"paymentPolicy={paymentPolicy}");
                    }

                    if (!string.IsNullOrEmpty(boardType))
                    {
                        queryParams.Add($"boardType={boardType}");
                    }

                    var queryString = string.Join("&", queryParams);
                    var url = $"https://test.api.amadeus.com/v3/shopping/hotel-offers?{queryString}";

                    var response = await _httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var offersResponse = JsonSerializer.Deserialize<HotelOffersResponse>(content, GetJsonOptions());
                        if (offersResponse?.Data != null)
                        {
                            allOffers.AddRange(offersResponse.Data);
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to get hotel offers for hotels {HotelIds}. Status: {StatusCode}, Error: {Error}", 
                            string.Join(",", batch), response.StatusCode, errorContent);
                        
                        // Try to parse error response for better logging
                        try
                        {
                            var errorResponse = JsonSerializer.Deserialize<AmadeusErrorResponse>(errorContent, GetJsonOptions());
                            if (errorResponse?.Errors?.Any() == true)
                            {
                                foreach (var error in errorResponse.Errors)
                                {
                                    _logger.LogWarning("Amadeus API Error: {Code} - {Title}: {Detail}", error.Code, error.Title, error.Detail);
                                }
                            }
                        }
                        catch
                        {
                            // Ignore error parsing failures
                        }
                        
                        // Continue with next batch instead of failing completely
                        continue;
                    }

                    // Add a small delay between requests to avoid rate limiting
                    await Task.Delay(100);
                }

                return allOffers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting hotel offers for hotels {HotelIds}", string.Join(",", hotelIds));
                return new List<HotelOffer>();
            }
        }

        public async Task<List<HotelSentiment>> GetHotelSentimentsAsync(List<string> hotelIds)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Limit to maximum 5 hotels per API call to avoid 400 error
                const int maxHotelsPerRequest = 5;
                var allSentiments = new List<HotelSentiment>();

                // Process hotels in batches
                for (int i = 0; i < hotelIds.Count; i += maxHotelsPerRequest)
                {
                    var batch = hotelIds.Skip(i).Take(maxHotelsPerRequest).ToList();
                    var hotelIdsParam = string.Join(",", batch);
                    
                    var response = await _httpClient.GetAsync($"https://test.api.amadeus.com/v2/e-reputation/hotel-sentiments?hotelIds={hotelIdsParam}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var sentimentsResponse = JsonSerializer.Deserialize<HotelSentimentsResponse>(content, GetJsonOptions());
                        if (sentimentsResponse?.Data != null)
                        {
                            allSentiments.AddRange(sentimentsResponse.Data);
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to get hotel sentiments for hotels {HotelIds}. Status: {StatusCode}, Error: {Error}", 
                            string.Join(",", batch), response.StatusCode, errorContent);
                        
                        // If we get an error, continue with next batch instead of failing completely
                        continue;
                    }

                    // Add a small delay between requests to avoid rate limiting
                    await Task.Delay(100);
                }

                return allSentiments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting hotel sentiments for hotels {HotelIds}", string.Join(",", hotelIds));
                return new List<HotelSentiment>();
            }
        }

        public async Task<List<Hotel>> SearchHotelsForDestinationAsync(HotelSearchRequest request)
        {
            List<Hotel> hotels = new List<Hotel>();

            // Primary search by city code
            if (!string.IsNullOrEmpty(request.CityCode))
            {
                hotels = await SearchHotelsByCityAsync(request.CityCode);
            }
            // Fallback to geocode search
            else if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                hotels = await SearchHotelsByGeoCodeAsync(
                    request.Latitude.Value, 
                    request.Longitude.Value,
                    request.Radius ?? 5,
                    request.RadiusUnit,
                    request.ChainCodes);
            }
            // Fallback to keyword search
            else if (!string.IsNullOrEmpty(request.Keyword))
            {
                hotels = await SearchHotelsByKeywordAsync(request.Keyword);
            }

            // If we have hotels, get their sentiments for ratings (limit to first 3 to avoid API limits)
            if (hotels.Any())
            {
                var hotelIds = hotels.Select(h => h.HotelId).Take(3).ToList(); // Further reduced limit to avoid API errors
                var sentiments = await GetHotelSentimentsAsync(hotelIds);
                
                // Add sentiment data to hotels
                foreach (var hotel in hotels)
                {
                    var sentiment = sentiments.FirstOrDefault(s => s.HotelId == hotel.HotelId);
                    if (sentiment != null)
                    {
                        // We can add this to the hotel object if needed, or handle it separately
                    }
                }
            }

            return hotels;
        }

        public async Task<HotelBookingResponse?> CreateHotelBookingAsync(HotelBookingRequest request)
        {
            try
            {
                var token = await _amadeusService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var jsonContent = JsonSerializer.Serialize(request, GetJsonOptions());
                var requestContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://test.api.amadeus.com/v1/booking/hotel-bookings", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var bookingResponse = JsonSerializer.Deserialize<HotelBookingResponse>(content, GetJsonOptions());
                    
                    _logger.LogInformation("Successfully created hotel booking with ID: {BookingId}", 
                        bookingResponse?.Data?.FirstOrDefault()?.Id ?? "Unknown");
                    
                    return bookingResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create hotel booking. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    
                    // Try to parse and log detailed error information
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<AmadeusErrorResponse>(errorContent, GetJsonOptions());
                        if (errorResponse?.Errors?.Any() == true)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                _logger.LogWarning("Hotel Booking API Error: {Code} - {Title}: {Detail}", 
                                    error.Code, error.Title, error.Detail);
                            }
                        }
                    }
                    catch
                    {
                        // Ignore error parsing failures
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating hotel booking");
                return null;
            }
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }

    // Additional DTO for keyword search
    public class HotelSearchResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<HotelSearchResult> Data { get; set; } = new List<HotelSearchResult>();
    }

    public class HotelSearchResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("iataCode")]
        public string IataCode { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("subType")]
        public string SubType { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("relevance")]
        public int Relevance { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("hotelIds")]
        public List<string> HotelIds { get; set; } = new List<string>();
        
        [System.Text.Json.Serialization.JsonPropertyName("address")]
        public Address Address { get; set; } = new Address();
        
        [System.Text.Json.Serialization.JsonPropertyName("geoCode")]
        public GeoCode GeoCode { get; set; } = new GeoCode();
    }
}