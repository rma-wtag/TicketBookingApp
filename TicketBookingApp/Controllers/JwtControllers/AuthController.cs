using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.Dtos.JWTDTOs;
using TicketBookingApp.Services.JWT_Services;

namespace TicketBookingApp.Controllers.JwtControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO registerDto)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState); 
            var success = await _userService.RegisterUserAsync(registerDto);
            
            if (!success)
                return BadRequest(new { message = "Email already exists." });
           
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO loginDto)
        {
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState); 
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
           
            var authResponse = await _userService.AuthenticateUserAsync(loginDto, ipAddress);
            
            if (authResponse == null)
                return Unauthorized(new { message = "Invalid credentials or client." });
            
            return Ok(authResponse);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDTO refreshRequest)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState); 
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
           
            var authResponse = await _userService.RefreshTokenAsync(refreshRequest.RefreshToken, refreshRequest.ClientId, ipAddress);
            
            if (authResponse == null)
                return Unauthorized(new { message = "Invalid refresh token or client." });
            
            return Ok(authResponse);
        }
    }
}
