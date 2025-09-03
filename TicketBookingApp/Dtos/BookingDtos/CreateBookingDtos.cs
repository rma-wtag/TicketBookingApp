using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.BookingDtos
{
    public class CreateBookingDtos
    {
        public int UserId { get; set; }
        public int ShowId { get; set; }
        public List<int> SelectedSeatIds { get; set; } = new List<int>();
    }
}
