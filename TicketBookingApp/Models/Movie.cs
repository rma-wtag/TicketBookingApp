using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Movie Title is required.")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Movie duration is required.")]
        public TimeSpan Duration { get; set; }
        public string? PosterUrl { get; set; }
        public List<Show>? Shows { get; set; }
    }
}
