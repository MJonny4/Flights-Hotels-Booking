using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class BookingHistoryViewModel
    {
        public List<BookingHistoryItem> Bookings { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public string? SearchTerm { get; set; }
        public BookingStatus? StatusFilter { get; set; }
    }

    public class BookingHistoryItem
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusBadgeClass => Status switch
        {
            BookingStatus.Pending => "bg-warning",
            BookingStatus.Confirmed => "bg-success",
            BookingStatus.Cancelled => "bg-danger",
            BookingStatus.Completed => "bg-info",
            _ => "bg-secondary"
        };
        public bool CanCancel => Status == BookingStatus.Pending && BookingDate.AddDays(-7) > DateTime.UtcNow;
        public string FlightInfo { get; set; } = string.Empty;
    }
}