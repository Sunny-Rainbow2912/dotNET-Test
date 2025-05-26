using Test.Models;
using Microsoft.EntityFrameworkCore;
namespace Test.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }


        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "First Post",
                    Content = "This is the content of the first post.",
                    CreatedAt = DateTime.Now
                }

            );
            
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 2,
                    Title = "Second Post",
                    Content = "This is the content of the second post.",
                    CreatedAt = DateTime.Now
                }
            );
        }
        
    



    }
}


