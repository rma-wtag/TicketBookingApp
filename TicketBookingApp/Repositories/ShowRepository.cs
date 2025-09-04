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
        public async Task<IEnumerable<Show>> GetAllAsync() {
            return await _context.Shows
                            .Include(s => s.Movie)
                            .Include(s => s.Hall)
                            .Include(s => s.Bookings)
                            .ToListAsync();
        }
        public async Task<Show?> GetByIdAsync(int id) {
            var show = await _context.Shows.Include(s => s.Movie)
                                           .Include(s => s.Hall)
                                           .Include(s => s.Bookings)
                                           .FirstOrDefaultAsync(s => s.Id == id);
            return show;
        }

        public async Task<Show> CreateNewShowAsync(Show show) {
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
