using System.ComponentModel.DataAnnotations;
using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.JWTDTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(100, ErrorMessage = "Password must be less than or equal to 100 characters.")]
        public required string Password { get; set; }
        [Required(ErrorMessage = "ClientId is required.")]
        public string ClientId { get; set; } = null!;
    }
}
