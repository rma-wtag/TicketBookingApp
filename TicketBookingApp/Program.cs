using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using TicketBookingApp.AzureServices;
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Dtos.ShowDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Helpers;
using TicketBookingApp.Repositories;
using TicketBookingApp.Services;
using TicketBookingApp.Services.BookingServices;
using TicketBookingApp.Services.JWT_Services;
using TicketBookingApp.Services.PaymentServices;
namespace JWTDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            builder.Services.AddHttpClient<SSLCommerzService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "TicketBookingApp_";
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<UnitOfWork>();

            //Azure Services
            builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]));
            builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();

            builder.Services.AddMemoryCache();
            
            builder.Services.AddSingleton<IClientCacheService, ClientCacheService>();
            
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<ILogService, LogService>();

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

            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactClient",
                    policy => policy.WithOrigins("https://localhost:7143")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials());
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
            app.UseCors("AllowReactClient");
            app.MapControllers();

            //to fix docker migration issue.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();

                if (!databaseCreator.Exists())
                {
                    Console.WriteLine("Database does not exist. Creating and applying migrations...");
                    dbContext.Database.Migrate();
                }
                else
                {
                    Console.WriteLine("Database exists. Skipping creation.");

                    // Optionally, run migrations only if explicitly requested
                    var applyMigrations = builder.Configuration.GetValue<bool>("APPLY_MIGRATIONS", false);
                    if (applyMigrations)
                    {
                        Console.WriteLine("Applying pending migrations to existing database...");
                        dbContext.Database.Migrate();
                    }
                }
            }

            app.Run();
        }
    }
}