using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TicketBookingApp.Controllers.JwtControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("userdata")]
        public IActionResult GetUserData()
        {
            
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            
            return Ok(new { message = $"Hello, {userEmail}! Your User ID is {userId}." });
        }
        [HttpGet("adminonly")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Welcome, Admin! You have access to this resource." });
        }

        [HttpGet("useronly")]
        [Authorize(Roles = "User")]
        public IActionResult UserOnly()
        {
            return Ok(new { message = "Welcome, User! You have access to this resource." });
        }
    }
}
