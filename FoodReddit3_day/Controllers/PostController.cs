using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;
using System.Text.Json;
using System.Text;

namespace FoodReddit3_day.Controllers
{
    public class PostController : Controller
    {
        private readonly FoodForumContext _db;
        private readonly IConfiguration _config;

        public PostController(FoodForumContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchQuery, int? communityId, string? sortOrder, int page = 1)
        {
            ViewBag.Communities = await _db.Communities.ToListAsync();
            ViewBag.CurrentCommunityId = communityId;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentSort = sortOrder;

            var posts = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Comments)
                .Include(p => p.Votes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchQuery.StartsWith("@"))
                {
                    var username = searchQuery.Substring(1);
                    posts = posts.Where(p => p.Author.Username.Contains(username));
                }
                else
                {
                    posts = posts.Where(p =>
                        p.Title.Contains(searchQuery) ||
                        (p.Body != null && p.Body.Contains(searchQuery)));
                }
            }

            if (communityId.HasValue)
                posts = posts.Where(p => p.CommunityId == communityId.Value);

            switch (sortOrder)
            {
                case "az":
                    posts = posts.OrderBy(p => p.Title);
                    break;
                case "hot":
                    posts = posts.OrderByDescending(p => p.Comments.Count);
                    break;
                default:
                    posts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            int pageSize = 5;
            var finalPosts = await posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "FetchAPI")
                return PartialView("_PostList", finalPosts);

            return View(finalPosts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "User");

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
                return RedirectToAction("Login", "User");

            
            ModelState.Remove("CommunityList");

            
            if (model.CommunityId == 0)
                ModelState.AddModelError("CommunityId", "Please select a community");

            if (ModelState.IsValid)
            {
                var newPost = new Post
                {
                    Title = model.Title,
                    Body = model.Body,
                    CommunityId = model.CommunityId,
                    Ingredient = model.Ingredient,
                    UserId = currentUserId.Value,
                    CreatedAt = DateTime.UtcNow,
                    Score = 0
                };

                _db.Posts.Add(newPost);
                await _db.SaveChangesAsync();

                
                string posterName = HttpContext.Session.GetString("Username") ?? "Someone";

                var notification = new Notification
                {
                    SenderId = currentUserId.Value,
                    ReceiverId = null,                         
                    Type = "new_recipe",
                    Message = $"posted a new recipe: {newPost.Title}",
                    PostId = newPost.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Post");
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
        public async Task<IActionResult> Vote(int postId, int value)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Please login first" });

            var post = await _db.Posts.FindAsync(postId);
            if (post == null)
                return Json(new { success = false, message = "Post not found" });

            int voteValue = value > 0 ? 1 : -1;

            var existingVote = await _db.Votes
                .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId.Value);

            if (existingVote != null)
            {
                if (existingVote.Value == voteValue)
                {
                    post.Score -= voteValue;
                    _db.Votes.Remove(existingVote);
                }
                else
                {
                    post.Score -= existingVote.Value;
                    post.Score += voteValue;
                    existingVote.Value = voteValue;
                }
            }
            else
            {
                _db.Votes.Add(new Vote
                {
                    PostId = postId,
                    UserId = userId.Value,
                    Value = voteValue
                });
                post.Score += voteValue;
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, newScore = post.Score });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var detail = await _db.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Votes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (detail == null)
                return RedirectToAction("Index");

            return View(detail);
        }


        [HttpPost]
        [Route("Post/GenerateRecipeFromGemini")]
        public async Task<IActionResult> GenerateRecipeFromGemini(string ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredients))
                return Json(new { success = false, message = "Please provide ingredients first." });

            
            string? apiKey = _config["GeminiApi:ApiKey"]?.Trim();

            if (string.IsNullOrEmpty(apiKey))
                return Json(new
                {
                    success = false,
                    message = "API Key not found — check appsettings.json at path: GeminiApi:ApiKey"
                });

            string prompt = $@"You are a world-class master chef. 
Create a recipe using STRICTLY ONLY these ingredients: {ingredients}.

CRITICAL RULES:
1. DO NOT add any extra ingredients (no vegetables, no extra spices, no liquids) unless they are in the provided list.
2. If only one ingredient is provided, create a dish that uses ONLY that ingredient in different ways (e.g., crispy, seared, boiled).
3. If it's impossible to make a dish, try your best with only what's given.

Please reply in English and format the output exactly like this:

🍽️ Recipe Name: [Catchy Recipe Name]

🏷️ Community: [Suggest the most relevant category e.g. Thai Kitchen, Italian Kitchen, Healthy Eats, Vegan World, etc.]

⏱️ Time: Prep X mins | Cook X mins

📝 Ingredients:
- [Ingredient 1 with quantity]
- [Ingredient 2 with quantity]

👨‍🍳 Instructions:
- [Step 1]
- [Step 2]

⭐ Chef's Tip: [Special cooking tip or pairing suggestion]";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 4096
                }
            };

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            try
            {
                string jsonBody = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini Error: {responseString}");
                    return Json(new
                    {
                        success = false,
                        message = $"Gemini API Error {(int)response.StatusCode}: {responseString}"
                    });
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    return Json(new { success = false, message = "Gemini returned no candidates in the response." });

                var firstCandidate = candidates[0];

                if (!firstCandidate.TryGetProperty("content", out var content2))
                    return Json(new { success = false, message = "No content found in the candidate." });

                if (!content2.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                    return Json(new { success = false, message = "No parts found in the content." });

                string generatedText = parts[0].GetProperty("text").GetString() ?? "No answer provided.";

                return Json(new { success = true, recipeText = generatedText });
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false, message = "Timeout — Gemini took longer than 30 seconds." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Json(new { success = false, message = $"System Error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
                return Json(new List<string>());

            
            var suggestions = await _db.Posts
                .Where(p => p.Title.Contains(term))
                .OrderBy(p => p.Title)
                .Take(5)
                .Select(p => p.Title)
                .Distinct()
                .ToListAsync();

            return Json(suggestions);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string body, int? parentCommentId = null)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Please login first" });

            if (string.IsNullOrWhiteSpace(body))
                return Json(new { success = false, message = "Comment cannot be empty" });

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId.Value,
                Text = body,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            string username = HttpContext.Session.GetString("Username") ?? "Unknown";

            return Json(new
            {
                success = true,
                commentId = comment.Id,
                username = username,
                date = comment.CreatedAt.ToString("dd MMM HH:mm"),
                text = body
            });
        }
    }
}