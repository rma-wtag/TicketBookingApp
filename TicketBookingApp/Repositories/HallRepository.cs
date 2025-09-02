using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Repositories
{
    public class HallRepository
    {
        private readonly ApplicationDbContext _context;
        public HallRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Hall?> GetHallByIdAsync(int id) {
            var hallInfo = await _context.Halls
                                .Include(h => h.Seats)
                                .FirstOrDefaultAsync(h => h.Id == id);
            return hallInfo;
        }
        public async Task<Hall> CreateHallAsync(Hall hall)
        {
            await _context.Halls.AddAsync(hall);
            await _context.SaveChangesAsync();

            // generate 40 seats for this hall
            var seats = new List<Seat>();
            for (int i = 1; i <= 40; i++)
            {
                seats.Add(new Seat
                {
                    SeatNumber = $"S{i:D2}",
                    HallId = hall.Id,
                    Hall = hall
                });
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();

            // attach seats to hall navigation property
            hall.Seats = seats;

            return hall;
        }

        public async Task<Hall?> UpdateHallAsync(int id, Hall hall)
        {
            var existingHall = await _context.Halls.FindAsync(id);
            if (existingHall == null)
                return null;

            existingHall.Name = hall.Name;

            _context.Halls.Update(existingHall);
            await _context.SaveChangesAsync();
            return existingHall;
        }

        public async Task<Hall?> DeleteHallAsync(int id)
        {
            var existingHall = await _context.Halls.FindAsync(id);
            if (existingHall == null)
                return null;

            _context.Halls.Remove(existingHall);
            await _context.SaveChangesAsync();

            return existingHall;
        }
    }

}
