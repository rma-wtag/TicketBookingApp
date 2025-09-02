using System.ComponentModel.DataAnnotations;
using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.JWTDTOs
{
    public class UserRegisterDTO
    {
        [Required]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string Password { get; set; }
    }
}
