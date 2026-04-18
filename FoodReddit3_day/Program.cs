using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ADD SERVICES (ขาเข้า)
// ==========================================

// ใช้ AddControllersWithViews เพื่อให้ Render หน้า .cshtml ได้
builder.Services.AddControllersWithViews();

// ตั้งค่า Database
builder.Services.AddDbContext<FoodForumContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ตั้งค่า Session (ความจำของเว็บ) - แก้ให้เหลืออันเดียวที่สมบูรณ์ที่สุด
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Swagger (เก็บไว้ดู API เล่นๆ หรือทดสอบหลังบ้านได้ครับ ไม่เสียหาย)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. CONFIGURE PIPELINE (ลำดับการทำงาน)
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// สำคัญมากสำหรับ MVC: ทำให้เว็บมองเห็นไฟล์ใน wwwroot (CSS, Images, JS)
app.UseStaticFiles();

app.UseRouting();

// ต้องวาง UseSession ไว้ก่อน UseAuthorization
app.UseSession();

app.UseAuthorization();

// ==========================================
// 3. ROUTING (การกำหนดหน้าแรก)
// ==========================================

// ตั้งค่าให้หน้าแรกของเว็บวิ่งไปที่ Controller ชื่อ Home และ Action ชื่อ Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();