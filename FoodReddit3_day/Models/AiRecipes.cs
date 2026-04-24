using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodReddit3_day.Models
{
    public class AiRecipe
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string IngredientsPrompt { get; set; } 

        [Required]
        public string GeneratedRecipe { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User Requester { get; set; }

 
        public int? PostId { get; set; }

        [ForeignKey("PostId")]
        public Post SharedPost { get; set; }
    }
}