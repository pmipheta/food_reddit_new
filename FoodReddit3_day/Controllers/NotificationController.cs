using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;

namespace FoodReddit3_day.Controllers
{
    public class NotificationController : Controller
    {
        private readonly FoodForumContext _db;

        public NotificationController(FoodForumContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, notifications = new List<object>(), unreadCount = 0 });

            var notifs = await _db.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Post)
                .Where(n =>
                    
                    (n.ReceiverId == null && n.SenderId != userId) ||
                    
                    n.ReceiverId == userId
                )
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.Id,
                    n.Type,
                    n.Message,
                    n.IsRead,
                    n.PostId,
                    PostTitle  = n.Post != null ? n.Post.Title : "",
                    SenderName = n.Sender != null ? n.Sender.Username : "Someone",
                    TimeAgo    = GetTimeAgo(n.CreatedAt)
                })
                .ToListAsync();

            int unreadCount = notifs.Count(n => !n.IsRead);

            return Json(new { success = true, notifications = notifs, unreadCount });
        }


        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            
            var unreadNotifs = await _db.Notifications
                .Where(n => !n.IsRead && (n.ReceiverId == userId || (n.ReceiverId == null && n.SenderId != userId)))
                .ToListAsync();

            foreach (var n in unreadNotifs)
            {
                n.IsRead = true; 
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notif = await _db.Notifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        private static string GetTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;
            if (diff.TotalMinutes < 1)  return "just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24)   return $"{(int)diff.TotalHours} hr ago";
            if (diff.TotalDays < 7)     return $"{(int)diff.TotalDays}d ago";
            return createdAt.ToString("dd MMM");
        }
    }
}
