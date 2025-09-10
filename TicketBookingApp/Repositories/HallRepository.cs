using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Services;

namespace TicketBookingApp.Repositories
{
    public class HallRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        private const string HallAllCacheKey = "halls:getall";
        private const string HallByIdCacheKey = "halls:";

        public HallRepository(ApplicationDbContext context, IDistributedCache cache, ILogService logService)
        {
            _context = context;
            _cache = cache;
            _logService = logService;
        }
        public async Task<IEnumerable<Hall>> GetAllAsync()
        {
            var cached = await _cache.GetStringAsync(HallAllCacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<IEnumerable<Hall>>(cached)!;
            }

            var hallInfo = await _context.Halls
                                         .Include(h => h.Seats)
                                         .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _cache.SetStringAsync(HallAllCacheKey, JsonConvert.SerializeObject(hallInfo, jsonSettings), cacheOptions);

            return hallInfo;
        }
        public async Task<Hall?> GetHallByIdAsync(int id)
        {
            string cacheKey = $"{HallByIdCacheKey}{id}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<Hall>(cached);
            }

            var hallInfo = await _context.Halls
                                         .Include(h => h.Seats)
                                         .FirstOrDefaultAsync(h => h.Id == id);

            if (hallInfo != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(hallInfo, jsonSettings), cacheOptions);
            }

            return hallInfo;
        }
        public async Task<Hall> CreateHallAsync(Hall hall)
        {
            await _context.Halls.AddAsync(hall);
            await _context.SaveChangesAsync(); // Save to generate Hall.Id

            // Generate 40 seats for this hall
            var seats = new List<Seat>();
            for (int i = 1; i <= 40; i++)
            {
                seats.Add(new Seat
                {
                    SeatNumber = $"S{i:D2}",
                    HallId = hall.Id
                });
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();

            hall.Seats = seats;

            await InvalidateCacheAsync();
            await _logService.LogAsync("CREATE","Hall",$"Created hall '{hall.Name}' with {seats.Count} seats.");

            return hall;
        }

        public async Task<Hall?> UpdateHallAsync(int id, Hall hall)
        {
            var existingHall = await _context.Halls.FindAsync(id);
            if (existingHall == null)
                return null;
            string oldName = existingHall.Name;
            existingHall.Name = hall.Name;
            _context.Halls.Update(existingHall);

            await _context.SaveChangesAsync();
            await InvalidateCacheAsync(id);

            await _logService.LogAsync("UPDATE", "Hall", $"Updated hall ID {id}: Name changed from '{oldName}' to '{hall.Name}'.");

            return existingHall;
        }

        public async Task<Hall?> DeleteHallAsync(int id)
        {
            var existingHall = await _context.Halls.FindAsync(id);
            if (existingHall == null)
                return null;

            _context.Halls.Remove(existingHall);
            await _context.SaveChangesAsync();

            await InvalidateCacheAsync(id);
            await _logService.LogAsync("DELETE", "Hall", $"Deleted hall '{existingHall.Name}' (ID: {id}).");
            return existingHall;
        }

        private async Task InvalidateCacheAsync(int? hallId = null)
        {
            await _cache.RemoveAsync(HallAllCacheKey);

            if (hallId != null)
            {
                string hallKey = $"{HallByIdCacheKey}{hallId}";
                await _cache.RemoveAsync(hallKey);
            }
        }
    }
}
