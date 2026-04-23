using FoodReddit3_day.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodReddit3_day.Data
{
    public class FoodForumContext : DbContext
    {
        public FoodForumContext(DbContextOptions<FoodForumContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<AiRecipe> AiRecipes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany() 
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.Cascade);
           

            modelBuilder.Entity<Community>().HasData(
                new Community { Id = 1, Name = "Thai Food", Description = "Spicy and flavorful dishes from Thailand" },
                new Community { Id = 2, Name = "Japanese Food", Description = "Sushi, ramen, and delicate flavors" },
                new Community { Id = 3, Name = "Korean Food", Description = "Kimchi, BBQ, and bold spicy dishes" },
                new Community { Id = 4, Name = "Chinese Food", Description = "Traditional and modern Chinese cuisine" },
                new Community { Id = 5, Name = "Italian Food", Description = "Pasta, pizza, and classic Italian flavors" },
                new Community { Id = 6, Name = "American Food", Description = "Burgers, fries, and comfort food" },
                new Community { Id = 7, Name = "Street Food", Description = "Delicious and affordable street eats" },
                new Community { Id = 8, Name = "Healthy Food", Description = "Clean eating and nutritious meals" },
                new Community { Id = 9, Name = "Vegan Food", Description = "Plant-based and cruelty-free dishes" },
                new Community { Id = 10, Name = "Seafood", Description = "Fresh fish, shrimp, and ocean delights" },
                new Community { Id = 11, Name = "BBQ & Grilled", Description = "Smoky grilled meats and BBQ dishes" },
                new Community { Id = 12, Name = "Desserts", Description = "Sweet treats, cakes, and pastries" },
                new Community { Id = 13, Name = "Cafe & Coffee", Description = "Coffee culture and cozy cafes" },
                new Community { Id = 14, Name = "Fast Food", Description = "Quick and convenient meals" },
                new Community { Id = 15, Name = "Home Cooking", Description = "Simple and homemade recipes" },
                new Community { Id = 16, Name = "Fine Dining", Description = "Luxury and high-end dining experiences" },
                new Community { Id = 17, Name = "Fusion Food", Description = "Creative mix of different cuisines" },
                new Community { Id = 18, Name = "Spicy Food", Description = "Hot and fiery dishes for spice lovers" },
                new Community { Id = 19, Name = "Snacks & Appetizers", Description = "Light bites and starters" },
                new Community { Id = 20, Name = "Drinks & Beverages", Description = "Juices, smoothies, and drinks" }
            );
        }
    }
}