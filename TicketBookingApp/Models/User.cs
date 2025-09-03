using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Username must be provided!")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Username must be provided!")]
        [EmailAddress(ErrorMessage = "E-mail address must be valid")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password must be provided!")]
        [MinLength(6,ErrorMessage = "Password must be atleast 6 digits long!")]
        [MaxLength(100)]
        public required string PasswordHash {get;set;}

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
