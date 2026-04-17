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
        public string IngredientsPrompt { get; set; } // e.g., "pork, rice, egg"

        [Required]
        public string GeneratedRecipe { get; set; } // The full recipe text from Gemini

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key: Who requested this recipe?
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User Requester { get; set; }

        // Optional Foreign Key: Did they share it to the board? 
        // If it's null, it's just saved privately in their account.
        public int? PostId { get; set; }

        [ForeignKey("PostId")]
        public Post SharedPost { get; set; }
    }
}