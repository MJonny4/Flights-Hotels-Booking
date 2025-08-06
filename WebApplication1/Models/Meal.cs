namespace WebApplication1.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MealType Type { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public ICollection<BookingFlight> BookingFlights { get; set; } = new List<BookingFlight>();
    }

    public enum MealType
    {
        Standard = 1,
        Vegetarian = 2,
        Vegan = 3,
        Halal = 4,
        Kosher = 5,
        GlutenFree = 6,
        LocalInspired = 7
    }
}