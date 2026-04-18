using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;
using FoodReddit3_day.Models;
using Microsoft.AspNetCore.Http; 
namespace FoodReddit3_day.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class UserController : Controller
    {
        private readonly FoodForumContext _db;

        public UserController(FoodForumContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login( LoginRequest request)
        {
            var user = await _db.User
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.PasswordHash);

            if (user == null)
            {
                
                ViewBag.ErrorMessage = "Username or Password Incorrect";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = await _db.User
                .FirstOrDefaultAsync(u => u.Username == request.Username );

            if (user != null)
            {
                ViewBag.ErrorMessage = "Username already exists";
                return View();
            }
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.PasswordHash,
                Email = request.Email
            };

            _db.User.Add(newUser);
            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }
    }
}
