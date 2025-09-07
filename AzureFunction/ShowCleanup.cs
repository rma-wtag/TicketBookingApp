using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TicketBookingApp.Entities;

namespace AzureFunction;

public class ShowCleanup
{
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _context;

    public ShowCleanup(ILoggerFactory loggerFactory, ApplicationDbContext context)
    {
        _logger = loggerFactory.CreateLogger<ShowCleanup>();
        _context = context;
    }

    [Function("ShowCleanup")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"ExpiredShowCleanupFunction executed at: {DateTime.UtcNow}");

        var now = DateTime.UtcNow;

        var expiredShows = _context.Shows
                .Where(s => s.EndTime < now);

        _context.Shows.RemoveRange(expiredShows);

        int rows = await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted {rows} expired shows.");
    }
}