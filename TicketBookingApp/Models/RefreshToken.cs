using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        [Required]
        public required string Token { get; set; }
        [Required]
        public required string JwtId { get; set; }
        [Required]
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public string? CreatedByIp { get; set; }
    }
}
