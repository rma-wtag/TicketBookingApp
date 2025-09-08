using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Repositories
{
    public class PaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(int pageNumber,int pageSize) {
            return await _context.Payments
                         .Include(p => p.Booking)
                             .ThenInclude(b => b.Show)
                                 .ThenInclude(s => s!.Movie)
                         .OrderByDescending(p => p.Id)
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
                         .ToListAsync();
        }
        public async Task<Payment?> GetById(int id)
        {
            var payment = await _context.Payments
                                .Include(p => p.Booking)
                                .ThenInclude(b => b.Show)
                                .ThenInclude(s => s!.Movie)
                                .FirstOrDefaultAsync(x => x.Id == id);

            if (payment == null) {
                return null;
            }

            return payment;
        }

        public async Task<IEnumerable<Payment>?> GetByUserId(int userId)
        {
            var payments = await _context.Payments
                                .Include(p => p.Booking)
                                .ThenInclude(b => b.Show)
                                .ThenInclude(s => s!.Movie)
                                .Where(x => x.Booking.UserId == userId)
                                .OrderByDescending(p => p.Id)
                                .ToListAsync();

            return payments;
        }

    }
}
