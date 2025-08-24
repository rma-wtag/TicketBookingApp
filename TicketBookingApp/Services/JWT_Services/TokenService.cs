using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services.JWT_Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user, IList<string> roles, out string jwtId, Client client)
        {

            var tokenHandler = new JwtSecurityTokenHandler();

            var keyBytes = Convert.FromBase64String(client.ClientSecret);
            var key = new SymmetricSecurityKey(keyBytes);

            jwtId = Guid.NewGuid().ToString();

            var issuer = _configuration["JwtSettings:Issuer"] ?? "DefaultIssuer";
            var accessTokenExpirationMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val) ? val : 15;

            var claims = new List<Claim>
            {
              
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
   
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
               
                new Claim(JwtRegisteredClaimNames.Aud, client.ClientURL),
               
                new Claim("client_id", client.ClientId)
            };
            
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes), 
                SigningCredentials = creds, 
                Issuer = issuer, 
                Audience = client.ClientURL 
            };
          
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
        
        public RefreshToken GenerateRefreshToken(string ipAddress, string jwtId, Client client, int userId)
        {
            
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            var refreshTokenExpirationDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpirationDays"], out var val) ? val : 7;
            
            return new RefreshToken
            {
                
                Token = Convert.ToBase64String(randomBytes),
                
                JwtId = jwtId,
                
                Expires = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                
                CreatedAt = DateTime.UtcNow,
                
                UserId = userId,
                
                ClientId = client.Id,
                
                IsRevoked = false,
                
                RevokedAt = null,
               
                CreatedByIp = ipAddress
            };
        }
    }
}
