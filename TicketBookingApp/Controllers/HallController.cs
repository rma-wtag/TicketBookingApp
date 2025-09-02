using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.Dtos.HallDtos;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HallController : ControllerBase
    {
        private readonly UnitOfWork _uow;
        private readonly IMapper _mapper;

        public HallController(UnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // CREATE hall
        [HttpPost]
        public async Task<ActionResult<Hall>> Create([FromBody] CreateHallDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hall = _mapper.Map<Hall>(createDto);
            var createdHall = await _uow.HallRepository.CreateHallAsync(hall);
            await _uow.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = createdHall.Id }, createdHall);
        }

        // GET hall by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Hall>> GetById(int id)
        {
            var hall = await _uow.HallRepository.GetHallByIdAsync(id);
            if (hall == null)
                return NotFound();
            return Ok(hall);
        }

        // UPDATE hall
        [HttpPut("{id}")]
        public async Task<ActionResult<Hall>> UpdateById(int id, [FromBody] UpdateHallDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hallEntity = _mapper.Map<Hall>(updateDto);
            var updatedHall = await _uow.HallRepository.UpdateHallAsync(id, hallEntity);

            if (updatedHall == null)
                return NotFound();

            await _uow.CommitAsync();
            return Ok(updatedHall);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Hall>> DeleteById([FromRoute]int id) {
            var deleted = await _uow.HallRepository.DeleteHallAsync(id);

            if (deleted == null) {
                return NotFound();
            }

            return Ok(deleted);
        }
    }

}
