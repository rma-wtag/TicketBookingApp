using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Models;

namespace TicketBookingApp.Entities
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User", Description = "Regular user role" },
                new Role { Id = 2, Name = "Admin", Description = "Administrator role" },
                new Role { Id = 3, Name = "Editor", Description = "Editor Role" }
            );
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    ClientId = "client-app-one", // Unique client identifier used in JWT tokens
                    Name = "Demo Client Application One",
                    ClientSecret = "fPXxcJw8TW5sA+S4rl4tIPcKk+oXAqoRBo+1s2yjUS4=", // Base64-encoded secret key
                    ClientURL = "https://clientappone.example.com", // Used as Audience in JWT validation
                    IsActive = true // Active client flag
                },
                new Client
                {
                    Id = 2,
                    ClientId = "client-app-two",
                    Name = "Demo Client Application Two",
                    ClientSecret = "UkY2JEdtWqKFY5cEUuWqKZut2o6BI5cf3oexOlCMZvQ=",
                    ClientURL = "https://clientapptwo.example.com",
                    IsActive = true
                }
            );
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;

    }
}
