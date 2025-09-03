using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.Services.PaymentServices;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _repo;
        public PaymentController(IPaymentRepository repository)
        {
            _repo = repository;
        }


    }
}
