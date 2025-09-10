using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Services;

namespace TicketBookingApp.Repositories
{
    public class MovieRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;

        private const string MovieAllCacheKey = "movies:getall";
        private const string MovieByIdCacheKey = "movies:";
        private const string MovieAllKeysTracker = "movies:getall:keys";

        public MovieRepository(ApplicationDbContext context, IDistributedCache cache,ILogService logService)
        {
            _context = context;
            _cache = cache;
            _logService = logService;
        }

        // READ - Get paginated movies with optional search
        public async Task<IEnumerable<Movie>> GetAllMoviesAsync(string? search, int pageNumber, int pageSize)
        {
            string cacheKey = $"{MovieAllCacheKey}:search={search ?? "null"}:page={pageNumber}:size={pageSize}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<IEnumerable<Movie>>(cached)!;
            }

            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            query = query.OrderByDescending(m => m.CreatedDate);

            var movies = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(movies, jsonSettings), cacheOptions);
            await TrackCacheKeyAsync(cacheKey);

            return movies;
        }

        // READ - Get single movie by id
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            string cacheKey = $"{MovieByIdCacheKey}{id}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<Movie>(cached);
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(movie, jsonSettings), cacheOptions);
                
            }
            return movie;
        }

        // CREATE - Add a single movie
        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            await InvalidateCacheAsync();
            await _logService.LogAsync("CREATE", "Movie", $"Created movie '{movie!.Title}' (ID: {movie.Id}, Rating: {movie.Rating}).");
            return movie;
        }

        // CREATE - Batch create multiple movies
        public async Task CreateMoviesAsync(IEnumerable<Movie> movies)
        {
            var movieList = movies.ToList();
            await _context.Movies.AddRangeAsync(movieList);
            await InvalidateCacheAsync();
            await _logService.LogAsync("CREATE","Movie",$"Created {movieList.Count} movies: {string.Join(", ", movieList.Select(m => m.Title))}.");
        }

        // UPDATE - Update movie by id
        public async Task<Movie?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto)
        {
            var existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
                return null;

            string oldValues = $"Old Description: {existingMovie.Description}, Old Duration: {existingMovie.Duration}, Old Rating: {existingMovie.Rating}";
            existingMovie.Description = updateMovieDto.Description;
            existingMovie.Duration = updateMovieDto.Duration;
            existingMovie.Rating = updateMovieDto.Rating;

            _context.Movies.Update(existingMovie);
            await InvalidateCacheAsync(id);
            await _logService.LogAsync("UPDATE","Movie",$"Updated movie '{existingMovie.Title}' (ID: {existingMovie.Id}). {oldValues} -> New Description: {updateMovieDto.Description}, New Duration: {updateMovieDto.Duration}, New Rating: {updateMovieDto.Rating}");
            return existingMovie;
        }

        // DELETE - Delete a movie by id
        public async Task<Movie?> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return null;

            _context.Movies.Remove(movie);
            await InvalidateCacheAsync(id);
            await _logService.LogAsync("DELETE","Movie",$"Deleted movie '{movie.Title}' (ID: {id}).");
            return movie;
        }

        private async Task TrackCacheKeyAsync(string key)
        {
            var keysJson = await _cache.GetStringAsync(MovieAllKeysTracker) ?? "[]";
            var keyList = JsonConvert.DeserializeObject<List<string>>(keysJson)!;

            if (!keyList.Contains(key))
            {
                keyList.Add(key);
                await _cache.SetStringAsync(MovieAllKeysTracker, JsonConvert.SerializeObject(keyList));
            }
        }

        private async Task InvalidateCacheAsync(int? movieId = null)
        {
            var keysJson = await _cache.GetStringAsync(MovieAllKeysTracker);
            if (!string.IsNullOrEmpty(keysJson))
            {
                var keyList = JsonConvert.DeserializeObject<List<string>>(keysJson)!;
                foreach (var key in keyList)
                {
                    await _cache.RemoveAsync(key);
                }

                await _cache.RemoveAsync(MovieAllKeysTracker);
            }

            if (movieId != null)
            {
                string movieKey = $"{MovieByIdCacheKey}{movieId}";
                await _cache.RemoveAsync(movieKey);
            }
        }
    }
}
