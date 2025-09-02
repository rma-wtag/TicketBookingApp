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
            return await _context.Halls.FirstOrDefaultAsync(h => h.Id == id);
        }
        public async Task<Hall> CreateHallAsync(Hall hall)
        {
            await _context.Halls.AddAsync(hall);
            await _context.SaveChangesAsync();
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
