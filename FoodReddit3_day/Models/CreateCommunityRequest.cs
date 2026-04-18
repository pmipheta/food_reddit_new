namespace FoodReddit3_day.Models
{
    public class CreateCommunityRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}