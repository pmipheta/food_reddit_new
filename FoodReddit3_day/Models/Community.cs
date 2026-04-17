using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodReddit3_day.Models
{
    public class Community
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        
        public ICollection<Post> Posts { get; set; }
    }
}