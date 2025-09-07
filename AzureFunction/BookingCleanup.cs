using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TicketBookingApp.Entities;

namespace AzureFunction;

public class BookingCleanup
{
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _context;

    public BookingCleanup(ILoggerFactory loggerFactory, ApplicationDbContext context)
    {
        _logger = loggerFactory.CreateLogger<BookingCleanup>();
        _context = context;
    }

    [Function("BookingCleanup")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Cleanup job executed at: {DateTime.UtcNow}");
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        var expiredBookings = _context.Bookings.Where(b => !b.IsCompleted && b.CreatedAt < cutoffTime);

        _context.Bookings.RemoveRange(expiredBookings);
        int rows = await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted {rows} expired bookings.");
    }
}