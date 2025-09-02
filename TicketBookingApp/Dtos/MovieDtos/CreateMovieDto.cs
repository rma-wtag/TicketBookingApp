using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketBookingApp.Models;

namespace TicketBookingApp.Dtos.MovieDtos
{
    public class CreateMovieDto
    {
        [Required(ErrorMessage = "Movie Title is required.")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10.0, ErrorMessage = "Rating should be between 0 to 10.")]
        public decimal Rating { get; set; }

        [Required(ErrorMessage = "Movie duration is required.")]
        public TimeSpan Duration { get; set; }
        public string? PosterUrl { get; set; }
    }
}
