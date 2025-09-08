using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TicketBookingApp.Entities;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string UserNotificationsCacheKey = "user:notifications:";
        public UserController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("notifications/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNotificationsById([FromRoute] int id) {

            string cacheKey = $"{UserNotificationsCacheKey}{id}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                var notifications = JsonSerializer.Deserialize<IEnumerable<string>>(cached);
                return Ok(notifications);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }

            var notificationsFromDb = user.Notifications ?? new List<string>();

            // Store in cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(notificationsFromDb), cacheOptions);

            return Ok(notificationsFromDb);
        }


    }
}
