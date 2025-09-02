using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Dtos.HallDtos
{
    public class UpdateHallDto
    {
        [Required(ErrorMessage = "Hall name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
