using TicketBookingApp.Dtos.BookingDtos;
using TicketBookingApp.Models;

namespace TicketBookingApp.Services.BookingServices
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<IEnumerable<Seat>?> GetAvailableSeatsAsync(int showId);
        Task<Booking?> DeleteBookingAsync(int id);
        Task<Booking?> CreateNewBookingAsync(CreateBookingDtos createBookingDtos);
    }
}
