using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Dtos.JWTDTOs;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services.JWT_Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IClientCacheService _clientCacheService;

        public UserService(ApplicationDbContext dbContext, ITokenService tokenService, IConfiguration configuration, IClientCacheService clientCacheService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _configuration = configuration;
            _clientCacheService = clientCacheService;
        }

        public async Task<AuthResponseDTO?> AuthenticateUserAsync(UserLoginDTO loginDto, string ipAddress)
        {
            
            var user = await _dbContext.Users
            .Include(ur => ur.Roles)
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null; 

            var roles = user.Roles.Select(ur => ur.Name).ToList();
           
            var client = await _clientCacheService.GetClientByIdAsync(loginDto.ClientId);
            if (client == null)
            {
                
                return null;
            }
           
            var accessToken = _tokenService.GenerateAccessToken(user, roles, out string jwtId, client);
            
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress, jwtId, client, user.Id);
          
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
            
            var accessTokenExpiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val) ? val : 15;
            
            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
            };
        }

        // Refreshes an expired access token using a valid refresh token and client ID
        public async Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken, string clientId, string ipAddress)
        {
            
            var client = await _clientCacheService.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return null;
            }
            var existingToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(ur => ur.Roles)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ClientId == client.Id);
            
            if (existingToken == null || existingToken.IsRevoked || existingToken.Expires <= DateTime.UtcNow)
                return null; 
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;
            var user = existingToken.User;
            var roles = user.Roles.Select(ur => ur.Name).ToList();
            
            var accessToken = _tokenService.GenerateAccessToken(user, roles, out string newJwtId, client);
            
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress, newJwtId, client, user.Id);
            
            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();
           
            var accessTokenExpiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val) ? val : 15;
           
            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
            };
        }

        public async Task<bool> RegisterUserAsync(UserRegisterDTO registerDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email)) return false;

            var user = new User { 
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            var userRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null) {
                user.Roles.Add(userRole);
            }

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        // Revokes an existing refresh token to prevent further use
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            
            var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            
            if (existingToken == null || existingToken.IsRevoked)
                return false;
            
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
