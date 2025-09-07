using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly UnitOfWork _uow;
        public PaymentController(UnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments() {
            var payments = await _uow.PaymentRepository.GetAllPaymentsAsync();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Task?>> GetById([FromRoute] int id) {
            var payment = await _uow.PaymentRepository.GetById(id);

            if (payment == null) {
                return NotFound();
            }

            return Ok(payment);
        }
    }
}
