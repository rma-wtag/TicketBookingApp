using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TicketBookingApp.Dtos.ShowDtos;
using TicketBookingApp.Helpers;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShowController : ControllerBase
    {
        private readonly UnitOfWork _uow;
        private readonly IMapper _mapper;
        public ShowController(UnitOfWork uow,IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Show>>> GetAll(DateTime? startTime, string? movieId, int pageNumber=1,int pageSize=10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and size must be greater than zero.");

            var shows = await _uow.ShowRepository.GetAllAsync(startTime, movieId, pageNumber, pageSize);
            return Ok(shows);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Show>> GetById([FromRoute] int id) {
            var show = await _uow.ShowRepository.GetByIdAsync(id);

            if (show == null)
                return NotFound();

            return Ok(show);
        }

        [HttpPost]
        public async Task<ActionResult<Show>> CreateShow([FromBody] CreateShowDto createShowDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var show = _mapper.Map<Show>(createShowDto);

            try
            {
                var createdShow = await _uow.ShowRepository.CreateNewShowAsync(show);
                await _uow.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = createdShow?.Id }, createdShow);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateShow([FromRoute] int id, [FromBody] UpdateShowDto updateShowDto) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var show = _mapper.Map<Show>(updateShowDto);
            var updatedShow = await _uow.ShowRepository.UpdateShowAsync(id,show);

            if(updatedShow == null) return NotFound();

            await _uow.CommitAsync();

            return Ok(updatedShow);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<Show>> DeleteById([FromRoute] int id) {
            var show = await _uow.ShowRepository.DeleteShowAsync(id);
            if (show == null) return NotFound();

            await _uow.CommitAsync();
            return Ok(show);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadShowsList(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            ISheetParser<CreateShowDto> parser = new ShowSheetParser(); // Manual instantiation

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            List<CreateShowDto> createShowDtos = extension switch
            {
                ".csv" => await parser.ParseCsvAsync(file),
                ".xlsx" => await parser.ParseExcelAsync(file),
                _ => throw new InvalidOperationException("Only CSV or Excel (.xlsx) files are supported.")
            };

            var showEntities = _mapper.Map<List<Show>>(createShowDtos);
            await _uow.ShowRepository.CreateMultipleShowsAsync(showEntities);
            await _uow.CommitAsync();

            return Ok(new { Count = showEntities.Count });
        }
    }
}
