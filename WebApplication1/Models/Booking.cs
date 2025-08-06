namespace WebApplication1.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string PassengerFirstName { get; set; } = string.Empty;
        public string PassengerLastName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public DateTime PassengerDateOfBirth { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<BookingFlight> BookingFlights { get; set; } = new List<BookingFlight>();
        public ICollection<HotelBooking> HotelBookings { get; set; } = new List<HotelBooking>();
    }
}