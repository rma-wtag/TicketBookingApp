using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Seat
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "SeatNumber is required.")]
        public string SeatNumber { get; set; } = string.Empty;
        public int HallId { get; set; }
        public Hall? Hall { get; set; }

        public List<BookingSeat?> BookingSeats { get; set; } = new();
    }
}