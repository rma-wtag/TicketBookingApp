using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "User Id is required")]
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required(ErrorMessage = "Show Id is required")]
        public int ShowId { get; set; }
        public Show? Show { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false;
        public required Payment Payment { get; set; }
        public List<BookingSeat> BookingSeats { get; set; } = new();
    }
}
