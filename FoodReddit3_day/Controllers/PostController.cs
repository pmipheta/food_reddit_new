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
        public async Task<IActionResult> AddComment(int postId, string body, int? parentCommentId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Please login first" });

            if (!string.IsNullOrWhiteSpace(body))
            {
                var newComment = new Comment
                {
                    PostId = postId,
                    UserId = userId.Value,
                    Text = body,
                    ParentCommentId = parentCommentId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Comments.Add(newComment);
                await _db.SaveChangesAsync();

                string username = HttpContext.Session.GetString("Username") ?? "Unknown";

                return Json(new
                {
                    success = true,
                    username = username,
                    text = newComment.Text,
                    date = newComment.CreatedAt.ToString("dd MMM HH:mm"),
                    commentId = newComment.Id
                });
            }

            return Json(new { success = false, message = "Comment cannot be empty" });
        }
    }
}