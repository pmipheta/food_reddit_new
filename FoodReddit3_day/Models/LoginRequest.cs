namespace FoodReddit3_day.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}