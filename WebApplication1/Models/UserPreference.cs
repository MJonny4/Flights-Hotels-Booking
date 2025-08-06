namespace WebApplication1.Models
{
    public class UserPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PreferenceType { get; set; } = string.Empty; // "PreferredMeal", "PreferredClass", "PreferredSeat"
        public string PreferenceValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
    }
}