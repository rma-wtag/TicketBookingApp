using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Globalization;
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Helpers;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly UnitOfWork _uow;
        private IMapper _mapper;

        public MovieController(UnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // GET all movies
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var movies = await _uow.MovieRepository.GetAllMovies(search,pageNumber,pageSize);
            return Ok(movies);
        }

        // GET movie by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetById(int id)
        {
            var movie = await _uow.MovieRepository.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();
            return Ok(movie);
        }

        // CREATE movie
        [HttpPost]
        public async Task<ActionResult<Movie>> Create([FromBody] CreateMovieDto createMovieDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var movie = _mapper.Map<Movie>(createMovieDto);
            var createdMovie = await _uow.MovieRepository.CreateMovieAsync(movie);
            await _uow.CommitAsync();

            if (createdMovie.Rating >= 8.0m) {
                using var httpClient = new HttpClient();
                var functionUrl = "http://localhost:7164/api/notifications/high-rated";
                var json = System.Text.Json.JsonSerializer.Serialize(createdMovie);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                await httpClient.PostAsync(functionUrl, content);
            }

            return CreatedAtAction(nameof(GetById), new { id = createdMovie.Id }, createdMovie);
        }

        // UPDATE movie
        [HttpPut("{id}")]
        public async Task<ActionResult<Movie>> Update([FromRoute] int id, [FromBody] UpdateMovieDto updateMovieDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedMovie = await _uow.MovieRepository.UpdateMovieAsync(id, updateMovieDto);
            if (updatedMovie == null)
                return NotFound();
            await _uow.CommitAsync();

            return Ok(updatedMovie);
        }

        // DELETE movie
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _uow.MovieRepository.DeleteMovieAsync(id);
            if (deleted == null)
                return NotFound();
            await _uow.CommitAsync();

            return Ok(deleted);
        }

        //Upload CSV files
        [HttpPost("upload")]
        public async Task<IActionResult> UploadMoviesList(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            ISheetParser<CreateMovieDto> parser = new MovieSheetParser();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            List<CreateMovieDto> createMovieDtos = extension switch
            {
                ".csv" => await parser.ParseCsvAsync(file),
                ".xlsx" => await parser.ParseExcelAsync(file),
                _ => throw new InvalidOperationException("Only CSV or Excel (.xlsx) files are supported.")
            };

            var movieEntities = _mapper.Map<List<Movie>>(createMovieDtos);
            await _uow.MovieRepository.CreateMoviesAsync(movieEntities);
            await _uow.CommitAsync();

            return Ok(new { Count = movieEntities.Count });
        }
    }
}
