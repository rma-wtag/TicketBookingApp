using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services.JWT_Services
{
    public class ClientCacheService : IClientCacheService
    {
        private const string CacheKeyPrefix = "Client_";
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;

        public ClientCacheService(IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
        }

        public async Task<Client?> GetClientByIdAsync(string clientId)
        {
            var cacheKey = CacheKeyPrefix + clientId;

            if (_memoryCache.TryGetValue<Client>(cacheKey, out var client))
            {
                return client;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            client = await dbContext.Clients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);

            if (client != null)
            {
                _memoryCache.Set(cacheKey, client, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }

            return client;
        }
    }
}
