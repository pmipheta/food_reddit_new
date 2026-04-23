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
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Json(new { success = false, message = "Please login first" });

            // Include Comments and Votes so EF Core knows about them
            var post = await _db.Posts
                .Include(p => p.Comments)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (post != null)
            {
                // 1. Remove related Notifications
                var relatedNotifications = _db.Notifications.Where(n => n.PostId == id);
                _db.Notifications.RemoveRange(relatedNotifications);

                // 2. Remove related Votes and Comments to satisfy Foreign Key constraints
                if (post.Votes != null && post.Votes.Any())
                    _db.RemoveRange(post.Votes);

                if (post.Comments != null && post.Comments.Any())
                    _db.RemoveRange(post.Comments);

                // 3. Delete the Post itself
                _db.Posts.Remove(post);

                await _db.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Post not found or unauthorized" });
        }

    }
}