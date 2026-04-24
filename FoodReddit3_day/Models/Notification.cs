namespace FoodReddit3_day.Models
{
    public class Notification
    {
        public int Id { get; set; }

       
        public int? ReceiverId { get; set; }
        public User? Receiver { get; set; }

        
        public int? SenderId { get; set; }
        public User? Sender { get; set; }

        
        public string Type { get; set; } = "new_recipe";

        public string Message { get; set; } = "";

        
        public int? PostId { get; set; }
        public Post? Post { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
