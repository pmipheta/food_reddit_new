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
        public async Task<IActionResult> Index(string? searchQuery) 
        {
            
            var query = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .AsQueryable(); 

            
            if (!string.IsNullOrEmpty(searchQuery))
            {
                
                query = query.Where(p => p.Title.Contains(searchQuery) || p.Body.Contains(searchQuery));
            }

            
            var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            
            ViewBag.SearchQuery = searchQuery;

            return View(posts);
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
                    Ingredient =model.Ingredient,
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
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return RedirectToAction("Login", "User");

            
            
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (post != null)
            {
                
                _db.Posts.Remove(post);
                await _db.SaveChangesAsync();
            }

            
            return RedirectToAction("Index", "Profile");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var detail = await _db.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)      
                        .ThenInclude(r => r.Author)   
                .FirstOrDefaultAsync(p => p.Id == id);

            if (detail == null)
            {
                return RedirectToAction("Index");
            }
            return View(detail);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string body, int? parentCommentId)
        {
            // 1. เช็คว่า Login หรือยัง
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            // 2. เช็คว่าไม่ได้พิมพ์คอมเมนต์ว่างๆ มา
            if (!string.IsNullOrWhiteSpace(body))
            {
                // 3. สร้างคอมเมนต์ใหม่ (เช็คชื่อตัวแปรใน Model ของคุณด้วยนะครับ ว่าใช้ Body หรือ Content)
                var newComment = new Comment
                {
                    PostId = postId,
                    UserId = userId.Value,
                    Text = body, // ข้อความคอมเมนต์
                    ParentCommentId = parentCommentId, // ถ้าเป็นการตอบกลับ จะมีเลข ID ของคอมเมนต์แม่ส่งมาด้วย
                    CreatedAt = DateTime.UtcNow
                };

                _db.Comments.Add(newComment);
                await _db.SaveChangesAsync();
            }

            // 4. เซฟเสร็จแล้ว ให้เด้งกลับไปที่หน้า Details ของโพสต์เดิม
            return RedirectToAction("Details", new { id = postId });
        }
    }
}
