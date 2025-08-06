using System.Text.Json;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Extensions
{
    public static class FlightExtensions
    {
        public static StopoverInfo? GetStopoverInfo(this Flight flight)
        {
            if (string.IsNullOrEmpty(flight.StopoverInfo))
                return null;

            try
            {
                return JsonSerializer.Deserialize<StopoverInfo>(flight.StopoverInfo);
            }
            catch
            {
                return null;
            }
        }

        public static string GetStopoverDescription(this Flight flight)
        {
            var stopoverInfo = flight.GetStopoverInfo();
            if (stopoverInfo?.Stopovers?.Any() != true)
                return "Direct flight";

            var stops = stopoverInfo.Stopovers.Select(s => s.CityName).ToList();
            
            if (stops.Count == 1)
                return $"1 stop in {stops[0]}";
            
            return $"{stops.Count} stops in {string.Join(", ", stops)}";
        }

        public static List<string> GetStopCities(this Flight flight)
        {
            var stopoverInfo = flight.GetStopoverInfo();
            return stopoverInfo?.Stopovers?.Select(s => s.CityName).ToList() ?? new List<string>();
        }
    }
}