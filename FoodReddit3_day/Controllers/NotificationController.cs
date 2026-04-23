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

        // ─────────────────────────────────────────────
        // GET /Notification/GetMyNotifications
        // ดึง notifications ของ user ที่ login อยู่
        // (broadcast "new_recipe" + notifications ส่วนตัว)
        // ─────────────────────────────────────────────
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
                    // broadcast ถึงทุกคน ยกเว้นตัวเอง
                    (n.ReceiverId == null && n.SenderId != userId) ||
                    // ส่งตรงถึง user นี้
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

        // ─────────────────────────────────────────────
        // POST /Notification/MarkAllRead
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false });

            // 🌟 ดึงแจ้งเตือนที่ยังไม่ได้อ่านทั้งหมด (ทั้งแบบส่งตรงถึงเรา และแบบ Broadcast ที่คนอื่นโพสต์)
            var unreadNotifs = await _db.Notifications
                .Where(n => !n.IsRead && (n.ReceiverId == userId || (n.ReceiverId == null && n.SenderId != userId)))
                .ToListAsync();

            foreach (var n in unreadNotifs)
            {
                n.IsRead = true; // เปลี่ยนเป็นอ่านแล้ว
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ─────────────────────────────────────────────
        // POST /Notification/MarkRead/{id}
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Helper: แปลงเวลาเป็น "2 min ago" ฯลฯ
        // ─────────────────────────────────────────────
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
