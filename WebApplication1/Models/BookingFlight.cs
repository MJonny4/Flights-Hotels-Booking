namespace WebApplication1.Models
{
    public class BookingFlight
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int FlightId { get; set; }
        public int? SeatId { get; set; }
        public int? MealId { get; set; }
        public FlightClass FlightClass { get; set; }
        public decimal Price { get; set; }
        public bool HasMeal { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
        public Flight Flight { get; set; } = null!;
        public Seat? Seat { get; set; }
        public Meal? Meal { get; set; }
    }
}