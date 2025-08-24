using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TicketBookingApp.Entities;
using TicketBookingApp.Services.JWT_Services;
namespace JWTDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddMemoryCache();
            
            builder.Services.AddSingleton<IClientCacheService, ClientCacheService>();
            
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            
            Lazy<IClientCacheService>? clientCacheInstance = null;
            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, 
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidateAudience = false, 
                    ValidateIssuerSigningKey = true, 
                    ValidateLifetime = true, 
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        
                        var jwtToken = new JwtSecurityToken(token);
                        
                        var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                        
                        if (string.IsNullOrEmpty(clientId) || clientCacheInstance == null)
                            return Enumerable.Empty<SecurityKey>();
                        
                        var client = clientCacheInstance.Value.GetClientByIdAsync(clientId).Result;
                        if (client == null)
                            return Enumerable.Empty<SecurityKey>();
                        
                        var keyBytes = Convert.FromBase64String(client.ClientSecret);
                        
                        return new[] { new SymmetricSecurityKey(keyBytes) };
                    }
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {

                        var clientId = context.Principal?.FindFirst("client_id")?.Value;
                        if (string.IsNullOrEmpty(clientId))
                        {

                            context.Fail("ClientId claim missing.");
                            return;
                        }
                        if (clientCacheInstance == null)
                        {
                            context.Fail("Client Cache Instance is null");
                            return;
                        }

                        var client = await clientCacheInstance.Value.GetClientByIdAsync(clientId);
                        if (client == null)
                        {
                            context.Fail("Invalid client.");
                            return;
                        }
                        var audClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Aud)?.Value;
                        if (audClaim != client.ClientURL)
                        {
                            
                            context.Fail("Invalid audience.");
                            return;
                        }
                    }
                };
            });
            var app = builder.Build();

            clientCacheInstance = new Lazy<IClientCacheService>(() =>
            app.Services.GetRequiredService<IClientCacheService>());

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
           
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}