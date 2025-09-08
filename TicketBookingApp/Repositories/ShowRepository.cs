using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Repositories
{
    public class ShowRepository
    {
        private readonly ApplicationDbContext _context;
        public ShowRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Show>> GetAllAsync(DateTime? startTime,string? movieId, int pageNumber,int pageSize) {

            var query = _context.Shows
                                .Include(s => s.Movie)
                                .Include(s => s.Hall)
                                .Include(s => s.Bookings)
                                .AsQueryable();

            if (startTime.HasValue) {
                query = query.Where(s => s.StartTime >= startTime.Value);
            }

            if (!string.IsNullOrEmpty(movieId))
            {
                var movieIdInt = int.Parse(movieId);
                query = query.Where(s => s.MovieId == movieIdInt);
            }

            query = query
                .Where(s => s.StartTime >= DateTime.Now)
                .OrderBy(s => s.StartTime);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<Show?> GetByIdAsync(int id) {
            var show = await _context.Shows.Include(s => s.Movie)
                                           .Include(s => s.Hall)
                                           .Include(s => s.Bookings)
                                           .FirstOrDefaultAsync(s => s.Id == id);
            return show;
        }

        public async Task<Show?> CreateNewShowAsync(Show show) {
            var movie = await _context.Movies.FindAsync(show.MovieId);
            if (movie == null) return null;

            show.EndTime = show.StartTime + movie.Duration;

            bool overlapExists = await _context.Shows
                .AnyAsync(s => s.HallId == show.HallId &&
                              (
                                  (show.StartTime >= s.StartTime && show.StartTime < s.EndTime) ||
                                  (show.EndTime > s.StartTime && show.EndTime <= s.EndTime) ||
                                  (show.StartTime <= s.StartTime && show.EndTime >= s.EndTime)
                              ));
            if (overlapExists)
            {
                throw new InvalidOperationException("A show already exists in this hall during the selected time range.");
            }

            await _context.Shows.AddAsync(show);

            return show;
        }
        public async Task CreateMultipleShowsAsync(IEnumerable<Show> shows)
        {
            await _context.Shows.AddRangeAsync(shows);
        }

        public async Task<Show?> UpdateShowAsync(int id,Show show) {
            var existingShow = await _context.Shows.FindAsync(id);
            if (existingShow == null) {
                return null;
            }

            existingShow.MovieId = show.MovieId;
            existingShow.HallId = show.HallId;
            existingShow.Price = show.Price;
            existingShow.StartTime  = show.StartTime;
            existingShow.EndTime = show.EndTime;

            return existingShow;
        }

        public async Task<Show?> DeleteShowAsync(int id) {
            var show = await _context.Shows.FindAsync(id);
            if (show == null) { return null; }

            _context.Shows.Remove(show);

            return show;
        }
    }
}
