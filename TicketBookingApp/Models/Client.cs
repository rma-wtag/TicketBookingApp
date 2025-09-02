using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public required string Name {  get; set; }
        [Required(ErrorMessage = "Client Identifier is required.")]
        [MaxLength(50)]
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        [Required]
        [MaxLength(200)]
        public required string ClientURL { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
