using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class BookingSeat
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Booking Id is required")]
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        [Required(ErrorMessage = "Seat Id is required")]
        public int SeatId { get; set; }
        public Seat? Seat { get; set; }

        [Required(ErrorMessage = "Show Id is required")]
        public int ShowId { get; set; }
        public Show? Show { get; set; }
    }
}
