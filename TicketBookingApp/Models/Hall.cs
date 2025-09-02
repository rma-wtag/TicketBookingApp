namespace TicketBookingApp.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public List<Show> Shows { get; set; } = new List<Show>();
        public List<Seat> Seats { get; set; } = new List<Seat>();
    }
}
