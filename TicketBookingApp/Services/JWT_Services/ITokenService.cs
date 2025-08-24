using TicketBookingApp.Models;

namespace TicketBookingApp.Services.JWT_Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IList<string> roles, out string jwtId, Client client);
        RefreshToken GenerateRefreshToken(string ipAddress, string jwtId, Client client, int userId);
    }
}
