using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;

namespace FoodReddit3_day.Controllers
{
    
    public class PostController : Controller
    {
        private readonly FoodForumContext _db;

        public PostController(FoodForumContext db)
        {
            _db = db;
        }

        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            
            var communities = await _db.Communities.ToListAsync();
            var viewModel = new PostCreateViewModel
            {
                CommunityList = communities.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                var newPost = new Post
                {
                    Title = model.Title,
                    Body = model.Body,
                    CommunityId = model.CommunityId,
                    UserId = currentUserId.Value,
                    CreatedAt = DateTime.UtcNow,
                    Score = 0
                };

                _db.Posts.Add(newPost);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            
            var communities = await _db.Communities.ToListAsync();
            model.CommunityList = communities.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(model);
        }
    }
}