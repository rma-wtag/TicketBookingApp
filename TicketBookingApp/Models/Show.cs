namespace TicketBookingApp.Models
{
    public class Show
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public required Movie Movie { get; set; }
        public int HallId { get; set; }
        public required Hall Hall { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public DateTime StartTime { get; set;} = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
    }
}
