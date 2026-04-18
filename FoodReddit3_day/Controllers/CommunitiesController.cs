using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;
using AspNetCoreGeneratedDocument;
namespace FoodReddit3_day.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
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
            var communities = await _db.Communities.ToListAsync();

            return View(communities);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommunityRequest request)
        {
            var exist = await _db.Communities.AnyAsync(u => u.Name == request.Name);
            if (exist)
            {
                ViewBag.Error = "This community have already";
                return View();
            }

            var newCommunities = new Community
            {
                Name = request.Name,
                Description = request.Description ?? ""
            };

            _db.Communities.Add(newCommunities);

            await _db.SaveChangesAsync();

            
            return RedirectToAction("Index");
        }
    }

}