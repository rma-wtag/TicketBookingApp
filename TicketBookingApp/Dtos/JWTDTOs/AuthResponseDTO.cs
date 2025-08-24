namespace TicketBookingApp.Dtos.JWTDTOs
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
