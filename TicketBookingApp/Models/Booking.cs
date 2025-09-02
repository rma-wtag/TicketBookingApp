using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public int ShowId { get; set; }
        public required Show Show { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted = false;
        public Payment? Payment { get; set; }
        public List<BookingSeat> BookingSeats { get; set; } = new();
    }
}
