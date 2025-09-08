using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;

namespace TicketBookingApp.Repositories
{
    public class MovieRepository
    {
        private readonly ApplicationDbContext _context;

        public MovieRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ
        public async Task<IEnumerable<Movie>> GetAllMovies(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            query = query.OrderByDescending(m => m.CreatedDate);

            return await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.SingleOrDefaultAsync(m => m.Id == id);
        }

        // CREATE
        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            return movie;
        }

        // UPDATE 
        public async Task<Movie?> UpdateMovieAsync(int id,UpdateMovieDto updateMovieDto)
        {
            var existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
                return null;

            // Update properties
            existingMovie.Description = updateMovieDto.Description;
            existingMovie.Duration = updateMovieDto.Duration;
            existingMovie.Rating = updateMovieDto.Rating;

            _context.Movies.Update(existingMovie);
            return existingMovie;
        }

        // DELETE
        public async Task<Movie?> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return null;

            _context.Movies.Remove(movie);
            return movie;
        }

        //Create movies batch
        public async Task CreateMoviesAsync(IEnumerable<Movie> movies)
        {
            await _context.Movies.AddRangeAsync(movies);
        }
    }

}
