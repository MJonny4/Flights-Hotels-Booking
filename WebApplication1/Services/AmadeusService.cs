using System.Text;
using System.Text.Json;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class AmadeusService : IAmadeusService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AmadeusService> _logger;
        private string? _accessToken;
        private DateTime _tokenExpiryTime;

        public AmadeusService(HttpClient httpClient, IConfiguration configuration, ILogger<AmadeusService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            var baseUrl = _configuration["Amadeus:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Check if we have a valid token
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryTime)
            {
                return _accessToken;
            }

            try
            {
                var apiKey = _configuration["Amadeus:ApiKey"];
                var apiSecret = _configuration["Amadeus:ApiSecret"];

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", apiKey ?? ""),
                    new KeyValuePair<string, string>("client_secret", apiSecret ?? "")
                });

                var response = await _httpClient.PostAsync("/v1/security/oauth2/token", requestContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AmadeusAuthResponse>(responseContent);

                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                    {
                        _accessToken = authResponse.AccessToken;
                        // Amadeus tokens are valid for 30 minutes, refresh 1 minute before expiry as per documentation
                        _tokenExpiryTime = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn - 60);
                        
                        _logger.LogInformation("Successfully obtained Amadeus access token, expires in {ExpiresIn} seconds", authResponse.ExpiresIn);
                        return _accessToken;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get Amadeus access token. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting Amadeus access token");
            }

            throw new InvalidOperationException("Failed to obtain Amadeus access token");
        }

        public async Task<FlightOffersSearchResponse?> SearchFlightsAsync(FlightSearchRequest request)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                
                var queryParams = new List<string>
                {
                    $"originLocationCode={request.OriginLocationCode}",
                    $"destinationLocationCode={request.DestinationLocationCode}",
                    $"departureDate={request.DepartureDate:yyyy-MM-dd}",
                    $"adults={request.Adults}",
                    $"currencyCode={request.CurrencyCode}"
                };

                if (request.ReturnDate.HasValue)
                {
                    queryParams.Add($"returnDate={request.ReturnDate.Value:yyyy-MM-dd}");
                }

                if (request.Children > 0)
                {
                    queryParams.Add($"children={request.Children}");
                }

                if (request.Infants > 0)
                {
                    queryParams.Add($"infants={request.Infants}");
                }

                // Use cabinRestrictions instead of travelClass for better alignment with Amadeus API
                queryParams.Add($"cabinRestrictions={request.TravelClass}");
                queryParams.Add($"nonStop={request.NonStop.ToString().ToLower()}");
                queryParams.Add($"maxPrice={request.MaxPrice}");
                queryParams.Add($"max={request.Max}");
                
                if (request.IncludedCheckedBagsOnly)
                {
                    queryParams.Add("includedCheckedBagsOnly=true");
                }
                
                if (request.AddOneWayOffers)
                {
                    queryParams.Add("addOneWayOffers=true");
                }

                var queryString = string.Join("&", queryParams);
                var requestUri = $"/v2/shopping/flight-offers?{queryString}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var flightSearchResponse = JsonSerializer.Deserialize<FlightOffersSearchResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully retrieved {Count} flight offers from Amadeus", 
                        flightSearchResponse?.Data?.Count ?? 0);

                    return flightSearchResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to search flights. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching flights");
            }

            return null;
        }

        public async Task<FlightOffersSearchResponse?> GetFlightPriceAsync(string flightOfferId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();

                var requestBody = new
                {
                    data = new
                    {
                        type = "flight-offers-pricing",
                        flightOffers = new[] { new { id = flightOfferId } }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/shopping/flight-offers/pricing");
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                httpRequest.Content = requestContent;

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var flightPriceResponse = JsonSerializer.Deserialize<FlightOffersSearchResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully retrieved flight price from Amadeus");
                    return flightPriceResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get flight price. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting flight price");
            }

            return null;
        }
    }
}