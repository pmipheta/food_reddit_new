using Microsoft.AspNetCore.Mvc.Rendering; 

namespace FoodReddit3_day.Models
{
    public class PostCreateViewModel
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public int CommunityId { get; set; }

        
        public List<SelectListItem>? CommunityList { get; set; }
    }
}