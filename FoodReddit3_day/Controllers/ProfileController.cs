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
        public async Task<IActionResult> Index(string? searchQuery) 
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

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile profileImage)
        {
            
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            if (profileImage != null && profileImage.Length > 0)
            {
                
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    
                    return RedirectToAction("Index");
                }

                
                string fileName = userId + "_" + DateTime.Now.Ticks + extension;

                
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);

                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                
                var user = await _db.User.FindAsync(userId);
                if (user != null)
                {
                    
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        string oldFilePath = Path.Combine(uploadDir, user.ProfileImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.ProfileImageUrl = fileName;
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }
    }
}