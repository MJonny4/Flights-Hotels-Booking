using WebApplication1.Models;
using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface IFlightService
    {
        Task<List<City>> GetAllCitiesAsync();
        Task<List<Models.Route>> GetAvailableRoutesAsync(int originCityId, int destinationCityId);
        Task<List<Flight>> SearchFlightsAsync(FlightSearchRequest request);
        Task<decimal> CalculateSeasonalPriceAsync(decimal basePrice, int originCityId, int destinationCityId, DateTime departureDate);
        Task<List<Meal>> GetAvailableMealsAsync();
        Task SeedInitialDataAsync();
    }
}