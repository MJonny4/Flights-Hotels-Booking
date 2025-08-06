using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class FlightSearchViewModel
    {
        [Required(ErrorMessage = "Please select origin city")]
        [Display(Name = "From")]
        public int OriginCityId { get; set; }

        [Required(ErrorMessage = "Please select destination city")]
        [Display(Name = "To")]
        public int DestinationCityId { get; set; }

        [Required(ErrorMessage = "Please select departure date")]
        [Display(Name = "Departure Date")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Required]
        [Range(1, 9, ErrorMessage = "Number of passengers must be between 1 and 9")]
        [Display(Name = "Passengers")]
        public int Passengers { get; set; } = 1;

        [Display(Name = "Travel Class")]
        public FlightClass TravelClass { get; set; } = FlightClass.Economy;

        [Display(Name = "Non-stop flights only")]
        public bool NonStopOnly { get; set; }

        public List<City> AvailableCities { get; set; } = new List<City>();
        public List<Flight> SearchResults { get; set; } = new List<Flight>();
        public bool HasSearched { get; set; }
    }
}