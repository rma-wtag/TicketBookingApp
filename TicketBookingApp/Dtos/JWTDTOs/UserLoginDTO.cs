using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.JWTDTOs
{
    public class UserLoginDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string ClientId { get; set; } = null!;
    }
}
