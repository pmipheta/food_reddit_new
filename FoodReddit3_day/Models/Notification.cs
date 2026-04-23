namespace FoodReddit3_day.Models
{
    public class Notification
    {
        public int Id { get; set; }

        // ผู้รับ notification (null = broadcast ถึงทุกคน)
        public int? ReceiverId { get; set; }
        public User? Receiver { get; set; }

        // ผู้ทำ action (คนที่โพสต์ recipe)
        public int? SenderId { get; set; }
        public User? Sender { get; set; }

        // ประเภท: "new_recipe", "comment", "upvote", "follow"
        public string Type { get; set; } = "new_recipe";

        public string Message { get; set; } = "";

        // link ไปยัง post ที่เกี่ยวข้อง
        public int? PostId { get; set; }
        public Post? Post { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
