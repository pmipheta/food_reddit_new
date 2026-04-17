using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace FoodReddit3_day.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int UserId { get; set; }

        [ForeignKey("UserId")] // แก้ปัญหาแบบเดียวกับ Post.cs
        public User Author { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        // Self-Referencing for Nested Replies
        public int? ParentCommentId { get; set; }
        public Comment ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; }
    }
}