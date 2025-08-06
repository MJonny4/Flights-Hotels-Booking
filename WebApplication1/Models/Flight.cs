using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public int? RouteId { get; set; }
        public int OriginCityId { get; set; }
        public int DestinationCityId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public int EconomySeats { get; set; }
        public int BusinessSeats { get; set; }
        public int EconomyAvailableSeats { get; set; }
        public int BusinessAvailableSeats { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? StopoverInfo { get; set; } // JSON string containing stopover details
        public int NumberOfStops { get; set; } = 0;

        // Navigation properties
        public Route? Route { get; set; }
        public City OriginCity { get; set; } = null!;
        public City DestinationCity { get; set; } = null!;
        public ICollection<BookingFlight> BookingFlights { get; set; } = new List<BookingFlight>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }

    public enum FlightClass
    {
        [Display(Name = "Economy")]
        Economy = 1,
        [Display(Name = "Business")]
        Business = 2
    }

    public enum BookingStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Confirmed")]
        Confirmed = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 3,
        [Display(Name = "Completed")]
        Completed = 4
    }
}