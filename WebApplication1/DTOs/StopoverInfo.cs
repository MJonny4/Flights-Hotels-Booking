namespace WebApplication1.DTOs
{
    public class StopoverInfo
    {
        public List<Stopover> Stopovers { get; set; } = new List<Stopover>();
    }

    public class Stopover
    {
        public string AirportCode { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public TimeSpan LayoverDuration { get; set; }
        public string Terminal { get; set; } = string.Empty;
    }
}