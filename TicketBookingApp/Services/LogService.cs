
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task LogAsync(string action, string entityName, string details)
        {
            var log = new Log
            {
                Action = action,
                EntityName = entityName,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
