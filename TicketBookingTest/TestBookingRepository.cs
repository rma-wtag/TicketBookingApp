using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Services.BookingServices;

namespace TicketBookingTest
{
    public class TestBookingRepository : BookingRepository
    {
        public TestBookingRepository(ApplicationDbContext context, IDistributedCache cache)
            : base(context, cache) { }

        public override async Task<IEnumerable<Seat>?> GetAvailableSeatsAsync(int showId)
        {
            var hallId = await _context.Shows
                .Where(sh => sh.Id == showId)
                .Select(sh => sh.HallId)
                .FirstOrDefaultAsync();

            if (hallId == 0) return null;

            var availableSeats = await _context.Seats
                .Where(s => s.HallId == hallId &&
                            !_context.BookingSeats.Any(bs => bs.SeatId == s.Id && bs.Booking!.ShowId == showId))
                .ToListAsync();

            return availableSeats;
        }
    }
}
