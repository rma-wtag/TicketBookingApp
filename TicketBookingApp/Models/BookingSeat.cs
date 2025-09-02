namespace TicketBookingApp.Models
{
    public class BookingSeat
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public required Booking Booking { get; set; }

        public int SeatId { get; set; }
        public required Seat Seat { get; set; }
    }
}
