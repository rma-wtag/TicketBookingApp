using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Repositories
{
    public class ShowRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string ShowAllCacheKey = "shows:getall";
        private const string ShowByIdCacheKey = "shows:";
        private const string ShowAllKeysTracker = "shows:getall:keys";

        public ShowRepository(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Show>> GetAllAsync(DateTime? startTime, string? movieId, int pageNumber, int pageSize)
        {
            string cacheKey = $"{ShowAllCacheKey}:start={startTime?.ToString("yyyyMMddHHmm") ?? "null"}:movie={movieId ?? "null"}:page={pageNumber}:size={pageSize}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<IEnumerable<Show>>(cached)!;
            }

            var query = _context.Shows
                                .Include(s => s.Movie)
                                .Include(s => s.Hall)
                                .Include(s => s.Bookings)
                                .AsQueryable();

            if (startTime.HasValue)
                query = query.Where(s => s.StartTime >= startTime.Value);

            if (!string.IsNullOrEmpty(movieId))
            {
                var movieIdInt = int.Parse(movieId);
                query = query.Where(s => s.MovieId == movieIdInt);
            }

            query = query
                .Where(s => s.StartTime >= DateTime.Now)
                .OrderBy(s => s.StartTime);

            var shows = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(shows, jsonSettings), cacheOptions);

            // Track this cache key
            await TrackCacheKeyAsync(cacheKey);

            return shows;
        }

        public async Task<Show?> GetByIdAsync(int id)
        {
            string cacheKey = $"{ShowByIdCacheKey}{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<Show>(cached);
            }

            var show = await _context.Shows
                                     .Include(s => s.Movie)
                                     .Include(s => s.Hall)
                                     .Include(s => s.Bookings)
                                     .FirstOrDefaultAsync(s => s.Id == id);

            if (show != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(show, jsonSettings), cacheOptions);
            }

            return show;
        }

        public async Task<Show?> CreateNewShowAsync(Show show)
        {
            var movie = await _context.Movies.FindAsync(show.MovieId);
            if (movie == null) return null;

            show.EndTime = show.StartTime + movie.Duration;

            bool overlapExists = await _context.Shows
                .AnyAsync(s => s.HallId == show.HallId &&
                              ((show.StartTime >= s.StartTime && show.StartTime < s.EndTime) ||
                               (show.EndTime > s.StartTime && show.EndTime <= s.EndTime) ||
                               (show.StartTime <= s.StartTime && show.EndTime >= s.EndTime)));

            if (overlapExists)
            {
                throw new InvalidOperationException("A show already exists in this hall during the selected time range.");
            }

            await _context.Shows.AddAsync(show);
            await InvalidateCacheAsync();

            return show;
        }

        public async Task CreateMultipleShowsAsync(IEnumerable<Show> shows)
        {
            await _context.Shows.AddRangeAsync(shows);
            await InvalidateCacheAsync();
        }

        public async Task<Show?> UpdateShowAsync(int id, Show show)
        {
            var existingShow = await _context.Shows.FindAsync(id);
            if (existingShow == null) return null;

            existingShow.MovieId = show.MovieId;
            existingShow.HallId = show.HallId;
            existingShow.Price = show.Price;
            existingShow.StartTime = show.StartTime;
            existingShow.EndTime = show.EndTime;

            await InvalidateCacheAsync(id);

            return existingShow;
        }

        public async Task<Show?> DeleteShowAsync(int id)
        {
            var show = await _context.Shows.FindAsync(id);
            if (show == null) return null;

            _context.Shows.Remove(show);
            await InvalidateCacheAsync(id);

            return show;
        }

        private async Task TrackCacheKeyAsync(string key)
        {
            var keysJson = await _cache.GetStringAsync(ShowAllKeysTracker) ?? "[]";
            var keyList = JsonConvert.DeserializeObject<List<string>>(keysJson)!;

            if (!keyList.Contains(key))
            {
                keyList.Add(key);
                await _cache.SetStringAsync(ShowAllKeysTracker, JsonConvert.SerializeObject(keyList));
            }
        }

        private async Task InvalidateCacheAsync(int? showId = null)
        {
            // Remove all cached GetAllAsync keys
            var keysJson = await _cache.GetStringAsync(ShowAllKeysTracker);
            if (!string.IsNullOrEmpty(keysJson))
            {
                var keyList = JsonConvert.DeserializeObject<List<string>>(keysJson)!;
                foreach (var key in keyList)
                {
                    await _cache.RemoveAsync(key);
                }

                // Clear the tracker
                await _cache.RemoveAsync(ShowAllKeysTracker);
            }

            // Remove single show cache if needed
            if (showId != null)
            {
                string showKey = $"{ShowByIdCacheKey}{showId}";
                await _cache.RemoveAsync(showKey);
            }
        }
    }
}
