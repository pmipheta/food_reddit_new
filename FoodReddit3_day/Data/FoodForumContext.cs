using FoodReddit3_day.Models; 
using Microsoft.EntityFrameworkCore;


namespace FoodReddit3_day.Data
{
    public class FoodForumContext : DbContext
    {
        public FoodForumContext(DbContextOptions<FoodForumContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<AiRecipe> AiRecipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. ของเดิมที่คุณมีอยู่แล้ว (ป้องกัน Comment ซ้อนกันลบตัวเอง)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. เพิ่มใหม่: ป้องกันการชนกันระหว่าง User กับ Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. เพิ่มใหม่: ป้องกันการชนกันระหว่าง User กับ Post 
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}