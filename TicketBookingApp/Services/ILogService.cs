namespace TicketBookingApp.Services
{
    public interface ILogService
    {
        Task LogAsync(string action, string entityName, string details);
    }
}
