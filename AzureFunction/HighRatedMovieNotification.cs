using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace AzureFunction;

public class HighRatedMovieNotification
{
    private readonly ILogger<HighRatedMovieNotification> _logger;
    private readonly ApplicationDbContext _context;

    public HighRatedMovieNotification(ILogger<HighRatedMovieNotification> logger,ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("HighRatedMovieNotification")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post" , Route = "notifications/high-rated")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var movie = System.Text.Json.JsonSerializer.Deserialize<Movie>(body);

        var users = await _context.Users.ToListAsync();
        foreach (var user in users)
        {
            user.Notifications.Add($"🔥 New high-rated movie: {movie!.Title} ({movie.Rating}/10)");
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Notified {Count} users about {Movie}", users.Count, movie!.Title);

        return new OkObjectResult($"Notified {users.Count} users");
    }
}