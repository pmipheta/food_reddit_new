using Microsoft.EntityFrameworkCore;
using FoodReddit3_day.Data;   // ชี้ไปที่โฟลเดอร์ Data ของโปรเจกต์ใหม่

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ADD SERVICES TO THE CONTAINER
// ==========================================

// ตั้งค่า Database
builder.Services.AddDbContext<FoodForumContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ตั้งค่า CORS สำหรับ React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ใช้สำหรับ API (ไม่มี Views)
builder.Services.AddControllers();

// ตั้งค่า Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. CONFIGURE THE HTTP REQUEST PIPELINE
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();