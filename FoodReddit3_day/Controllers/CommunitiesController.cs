using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;
using AspNetCoreGeneratedDocument;
namespace FoodReddit3_day.Controllers
{

    public class CommunitiesController : Controller
    {
        private readonly FoodForumContext _db;

        public CommunitiesController(FoodForumContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            var communities = await _db.Communities
                                       .Include(c => c.Posts)
                                       .ToListAsync();

            return View(communities);
        }
    }

}