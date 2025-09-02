using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Globalization;
using TicketBookingApp.Dtos.MovieDtos;
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
        public ActionResult<IEnumerable<Movie>> GetAll()
        {
            var movies = _uow.MovieRepository.GetAllMovies();
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
        public async Task<IActionResult> UploadMoviesList(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest("No file uploaded.");
            }

            List<CreateMovieDto> createMovieDtos;

            if (Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)) {
                createMovieDtos = await ParseCsvAsync(file);
            }
            else if (Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                createMovieDtos = await ParseExcelAsync(file);
            }
            else
            {
                return BadRequest("Only CSV or Excel files are supported.");
            }

            var movieEntities = _mapper.Map<List<Movie>>(createMovieDtos);
            await _uow.MovieRepository.CreateMoviesAsync(movieEntities);
            await _uow.CommitAsync();

            return Ok(new { Count = movieEntities.Count });
        }

        private async Task<List<CreateMovieDto>> ParseCsvAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<CreateMovieDto>().ToList();
            return records;
        }
        private async Task<List<CreateMovieDto>> ParseExcelAsync(IFormFile file)
        {
            var movies = new List<CreateMovieDto>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // First worksheet
            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                movies.Add(new CreateMovieDto
                {
                    Title = row.Cell(1).GetString(),
                    Description = row.Cell(2).GetString(),
                    Rating = decimal.Parse(row.Cell(3).GetString()),
                    Duration = TimeSpan.Parse(row.Cell(4).GetString()),
                    PosterUrl = row.Cell(5).GetString()
                });
            }

            return movies;
        }

    }

}
