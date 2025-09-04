using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketBookingApp.Models
{
    public class Show
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "MovieId is required.")]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
        [Required(ErrorMessage = "HallId is required.")]
        public int HallId { get; set; }
        public Hall? Hall { get; set; }
        [Required(ErrorMessage = "Price is required.")]
        [Range(0,double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public DateTime StartTime { get; set;} = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
    }
}
