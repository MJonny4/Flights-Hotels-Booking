namespace WebApplication1.Models
{
    public class HotelBooking
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public int CityId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public string ExternalBookingReference { get; set; } = string.Empty;
        public string BookingProvider { get; set; } = string.Empty; // "Booking.com", "Hotels.com", etc.

        // Navigation properties
        public Booking Booking { get; set; } = null!;
        public City City { get; set; } = null!;
    }
}