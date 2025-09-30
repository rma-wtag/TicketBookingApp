using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TicketBookingApp.Dtos.BookingDtos;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;
using TicketBookingApp.Services.BookingServices;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;
        public BookingController(IBookingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll() {
            var bookings = await _repo.GetAllBookingAsync();

            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetById([FromRoute] int id)
        {
            var booking = await _repo.GetBookingByIdAsync(id);
            if(booking == null) return NotFound();

            return Ok(booking);
        }

        [HttpGet("GenerateTicket/{id}")]
        public async Task<IActionResult> GenerateTicketByBookingIdAsync([FromRoute] int id)
        {
            var result = await _repo.GenerateTicketByBookingIdAsync(id);

            if (result == null)
            {
                return NotFound("Booking not found or not completed");
            }

            return File(result.Value.pdfBytes, "application/pdf", result.Value.fileName);
        }

        //To book, I need to get available seats for a specific show, using showId and HallId
        [HttpGet("AvailableSeats/{showId}")]
        public async Task<ActionResult<IEnumerable<Seat>>> GetAvailableSeats([FromRoute] int showId) {
            var availableSeats = await _repo.GetAvailableSeatsAsync(showId);

            return Ok(availableSeats);
        }

        //Then choose not booked seats , that we get from BookingSeat table

        [HttpPost]
        [EnableRateLimiting("fixedOnIP")]
        public async Task<ActionResult<Booking>> CreateNewBooking([FromBody]CreateBookingDtos createBookingDtos) {
            
            var newBooking = await _repo.CreateNewBookingAsync(createBookingDtos);

            if (newBooking == null) {
                return NotFound();
            }

            return Ok(newBooking);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Booking?>> DeleteBooking([FromRoute] int id) {
            var deleted = await _repo.DeleteBookingAsync(id); 
            if (deleted == null) return NotFound();

            return Ok(deleted);
        }
    }
}
