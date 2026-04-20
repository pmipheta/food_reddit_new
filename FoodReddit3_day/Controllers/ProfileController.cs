using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;

namespace FoodReddit3_day.Controllers
{
    public class ProfileController : Controller
    {
        private readonly FoodForumContext _db;

        public ProfileController(FoodForumContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchQuery) // 1. รับค่า searchQuery
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            
            var user = await _db.User
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "User");

            
            var myPosts = user.Posts.AsQueryable();

            
            if (!string.IsNullOrEmpty(searchQuery))
            {
                myPosts = myPosts.Where(p =>
                    p.Title.Contains(searchQuery) ||
                    (p.Body != null && p.Body.Contains(searchQuery))
                );
            }

           
            var viewModel = new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email ?? "Cant find Email",
                ProfileImageUrl = user.ProfileImageUrl,
                MyPosts = myPosts.OrderByDescending(p => p.CreatedAt).ToList()
            };

            
            ViewBag.SearchQuery = searchQuery;

            return View(viewModel);
        }
    }
}