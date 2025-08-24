using TicketBookingApp.Models;

namespace TicketBookingApp.Services.JWT_Services
{
    public interface IClientCacheService
    {
        Task<Client?> GetClientByIdAsync(string clientId);
    }
}
