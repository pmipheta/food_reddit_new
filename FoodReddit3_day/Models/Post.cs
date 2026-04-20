using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FoodReddit3_day.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        public string Body { get; set; }

        public int Score { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Ingredient { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")] 
        public User Author { get; set; }

        public int CommunityId { get; set; }
        public Community Community { get; set; }

        // Navigation Properties
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Vote> Votes { get; set; }

        
        public AiRecipe AiRecipe { get; set; }
    }
}