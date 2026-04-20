namespace FoodReddit3_day.Models
{
    public class ProfileViewModel
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }

        
        public IEnumerable<Post> MyPosts { get; set; } = new List<Post>();
    }
}