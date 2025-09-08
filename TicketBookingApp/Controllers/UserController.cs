using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.Entities;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("notifications/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNotificationsById([FromRoute] int id) {
            var user = await _context.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }
            return user.Notifications;
        }

    }
}
