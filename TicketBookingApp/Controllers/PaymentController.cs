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
        public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments(int pageNumber=1,int pageSize=10) {
            var payments = await _uow.PaymentRepository.GetAllPaymentsAsync(pageNumber,pageSize);

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment?>> GetById([FromRoute] int id) {
            var payment = await _uow.PaymentRepository.GetById(id);

            if (payment == null) {
                return NotFound();
            }

            return Ok(payment);
        }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Payment>?>> GetByUserId([FromRoute] int userId)
        {
            var payments = await _uow.PaymentRepository.GetByUserId(userId);

            if (payments == null)
            {
                return NotFound();
            }

            return Ok(payments);
        }
    }
}
