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

            modelBuilder.Entity<Show>()
                .HasOne(sh => sh.Movie)
                .WithMany(m => m.Shows)
                .HasForeignKey(sh => sh.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Show>()
                .HasOne(sh => sh.Hall)
                .WithMany(h => h.Shows)
                .HasForeignKey(sh => sh.HallId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany(s => s.BookingSeats)
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentStatus)
                .HasConversion<string>();
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingSeat> BookingSeats { get; set; } = null!;
        public DbSet<Hall> Halls { get; set; } = null!;
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Seat> Seats { get; set; } = null!;
        public DbSet<Show> Shows { get; set; } = null!;

    }
}
