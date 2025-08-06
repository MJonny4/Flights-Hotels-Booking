using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using WebApplication1.DTOs;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1.ViewModels
{
    public class BookingViewModel
    {
        public int FlightId { get; set; }
        
        [JsonIgnore]
        [BindNever]
        public Flight? Flight { get; set; }
        
        [Display(Name = "Number of Passengers")]
        public int PassengerCount { get; set; } = 1;
        
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string PassengerFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string PassengerLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passport number is required")]
        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime PassengerDateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Contact Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a travel class")]
        [Display(Name = "Travel Class")]
        public FlightClass SelectedClass { get; set; }

        [Display(Name = "Seat Preference")]
        public int? PreferredSeatId { get; set; }

        [Display(Name = "Selected Seats")]
        public List<int> SelectedSeatIds { get; set; } = new List<int>();

        [Display(Name = "Meal Preference")]
        public int? PreferredMealId { get; set; }

        [Display(Name = "Include Meal")]
        public bool IncludeMeal { get; set; }

        public List<SeatDto> AvailableSeats { get; set; } = new List<SeatDto>();
        public List<Meal> AvailableMeals { get; set; } = new List<Meal>();
        
        public decimal TotalPrice { get; set; }
    }
}