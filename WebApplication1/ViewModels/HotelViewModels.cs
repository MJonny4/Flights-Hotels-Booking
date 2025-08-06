using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using WebApplication1.DTOs;

namespace WebApplication1.ViewModels
{
    public class HotelSearchViewModel
    {
        [Required(ErrorMessage = "Please select a destination city")]
        [Display(Name = "Destination City")]
        public int SelectedCityId { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        [Display(Name = "Check-in Date")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Check-out date is required")]
        [Display(Name = "Check-out Date")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(3);

        [Required(ErrorMessage = "Number of adults is required")]
        [Display(Name = "Adults")]
        [Range(1, 10, ErrorMessage = "Number of adults must be between 1 and 10")]
        public int Adults { get; set; } = 1;

        [Required(ErrorMessage = "Number of rooms is required")]
        [Display(Name = "Rooms")]
        [Range(1, 5, ErrorMessage = "Number of rooms must be between 1 and 5")]
        public int Rooms { get; set; } = 1;

        public List<City> AvailableCities { get; set; } = new List<City>();
    }

    public class HotelSearchResultViewModel
    {
        public HotelSearchViewModel SearchRequest { get; set; } = new HotelSearchViewModel();
        public List<Hotel> Hotels { get; set; } = new List<Hotel>();
        public string DestinationCity { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Adults { get; set; }
        public int Rooms { get; set; }
        public int TotalHotels { get; set; }
        public int DisplayedCount { get; set; }
        public bool HasMore { get; set; }
        public int TotalNights => (CheckOutDate - CheckInDate).Days;
    }

    public class HotelDetailsViewModel
    {
        public string HotelId { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string ChainCode { get; set; } = string.Empty;
        public Address? Address { get; set; }
        public List<Offer> Offers { get; set; } = new List<Offer>();
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Adults { get; set; }
        public int Rooms { get; set; }
        public bool HasOffers { get; set; }
        public HotelSentiment? Sentiment { get; set; }
        public int TotalNights => (CheckOutDate - CheckInDate).Days;
    }

    public class HotelBookingViewModel
    {
        public string OfferId { get; set; } = string.Empty;
        public string HotelId { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Adults { get; set; }
        public int Rooms { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = string.Empty;

        // Guest Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string GuestFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string GuestLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string GuestPhone { get; set; } = string.Empty;

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        public int TotalNights => (CheckOutDate - CheckInDate).Days;
    }
}