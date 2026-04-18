namespace FoodReddit3_day.Models
{
    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? Email { get; set; } 
    }
}