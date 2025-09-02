using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Hall
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Hall name is required.")]
        public required string Name { get; set; }
        public List<Show?> Shows { get; set; } = new List<Show>();
        public List<Seat?> Seats { get; set; } = new List<Seat>();
    }
}
