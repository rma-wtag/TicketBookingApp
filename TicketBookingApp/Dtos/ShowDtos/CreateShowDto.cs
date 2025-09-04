using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.ShowDtos
{
    public class CreateShowDto
    {
        [Required(ErrorMessage = "Movie Id is required.")]
        public int MovieId { get; set; }
        [Required(ErrorMessage = "Hall Id is required.")]
        public int HallId { get; set; }
        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
    }
}
