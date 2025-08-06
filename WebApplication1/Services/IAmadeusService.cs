using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAmadeusService
    {
        Task<string> GetAccessTokenAsync();
        Task<FlightOffersSearchResponse?> SearchFlightsAsync(FlightSearchRequest request);
        Task<FlightOffersSearchResponse?> GetFlightPriceAsync(string flightOfferId);
    }
}