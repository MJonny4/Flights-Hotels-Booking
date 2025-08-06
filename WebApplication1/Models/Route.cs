namespace WebApplication1.Models
{
    public class Route
    {
        public int Id { get; set; }
        public int OriginCityId { get; set; }
        public int DestinationCityId { get; set; }
        public bool RequiresTransfer { get; set; }
        public int? TransferCityId { get; set; }
        public decimal BasePrice { get; set; }
        public int FlightDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public City OriginCity { get; set; } = null!;
        public City DestinationCity { get; set; } = null!;
        public City? TransferCity { get; set; }
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}