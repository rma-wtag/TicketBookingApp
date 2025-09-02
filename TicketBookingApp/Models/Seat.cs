using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Seat
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "SeatNumber is required.")]
        public required string SeatNumber { get; set; }
        public int HallId { get; set; }
        public required Hall Hall { get; set; }
        public List<BookingSeat?> BookingSeats { get; set; } = new();

    }
}