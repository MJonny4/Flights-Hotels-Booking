namespace WebApplication1.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public FlightClass SeatClass { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsWindowSeat { get; set; }
        public bool IsAisleSeat { get; set; }

        // Navigation properties
        public Flight Flight { get; set; } = null!;
        public ICollection<BookingFlight> BookingFlights { get; set; } = new List<BookingFlight>();
    }
}