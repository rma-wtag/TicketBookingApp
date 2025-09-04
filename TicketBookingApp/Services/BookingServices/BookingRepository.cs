using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Dtos.BookingDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services.BookingServices
{
    public class BookingRepository : IBookingRepository
    {
        public readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingAsync()
        {
            return await _context.Bookings.Include(b => b.Payment)
                                            .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id){
            var booking = await _context.Bookings.Include(b => b.Payment)
                                                 .FirstOrDefaultAsync(x => x.Id == id);

            if (booking == null) return null;
            return booking;
        }

        public async Task<IEnumerable<Seat>?> GetAvailableSeatsAsync(int showId)
        {
            var hallId = await _context.Shows.Where(sh => sh.Id == showId)
                                        .Select(sh => sh.HallId)
                                        .FirstOrDefaultAsync();
            if (hallId == 0) return null;

            var availableSeats = await _context.Seats
                                        .Where(s=> s.HallId == hallId &&  !_context.BookingSeats
                                        .Any(bs => bs.SeatId == s.Id && bs.Booking!.ShowId == showId))
                                        .ToListAsync();
            return availableSeats;
        }

        public async Task<Booking?> CreateNewBookingAsync(CreateBookingDtos createBookingDtos)
        {
            var selectedIds = createBookingDtos.SelectedSeatIds.Distinct().ToList();
            if (selectedIds.Count == 0) return null;
            //if (selectedIds.Count > 4) return null; // need to handle, from user pov

            var availableSeats = await GetAvailableSeatsAsync(createBookingDtos.ShowId);
            var availableSeatIds = availableSeats!.Select(s => s.Id);

            if (!selectedIds.All(id => availableSeatIds.Contains(id)))
                return null;

            using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                var takenNow = await _context.BookingSeats
                    .Where(bs => bs.ShowId == createBookingDtos.ShowId && selectedIds.Contains(bs.SeatId))
                    .Select(bs => bs.SeatId)
                    .ToListAsync();

                if (takenNow.Any())
                {
                    await tx.RollbackAsync();
                    return null;
                }

                var show = await _context.Shows.FirstOrDefaultAsync(s => s.Id == createBookingDtos.ShowId);

                var booking = new Booking
                {
                    UserId = createBookingDtos.UserId,
                    ShowId = createBookingDtos.ShowId,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    Payment = new Payment
                    {
                        Amount = (show!.Price * selectedIds.Count),
                        PaymentStatus = PaymentStatus.Processing,
                        DateTime = DateTime.UtcNow
                    }
                };

                foreach (var seatId in selectedIds)
                {
                    booking.BookingSeats.Add(new BookingSeat
                    {
                        SeatId = seatId,
                        ShowId = createBookingDtos.ShowId
                    });
                }

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return booking;
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return null;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<Booking?> DeleteBookingAsync(int id) { 
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) { return null; }

            _context.Remove(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
    }
}
