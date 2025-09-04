using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Entities;
using TicketBookingApp.Models.PaymentGwModels;
using TicketBookingApp.Services.PaymentServices;

namespace TicketBookingApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly SSLCommerzService _ssl;
        private readonly ApplicationDbContext _context;
        public PaymentController(SSLCommerzService ssl,ApplicationDbContext context)
        {
            _ssl = ssl;
            _context = context;
        }

        /// Step 1: Create hosted checkout session
        [HttpPost("session")]
        public async Task<ActionResult<PaymentSessionResponse>> CreateSession([FromBody] PaymentSessionRequest req, CancellationToken ct)
        {
            var payload = new Dictionary<string, string>
        {
            { "total_amount", req.Amount.ToString("0.0") },
            { "currency", "BDT" },
            { "tran_id", req.PaymentId.ToString() },

            { "success_url", "https://localhost:7064/api/v1/Payment/success" },
            { "fail_url", "https://localhost:7064/api/v1/Payment/fail" },
            { "cancel_url", "https://localhost:7064/api/v1/Payment/cancel" },
            { "cus_name", req.CusName },
            { "cus_email", req.CusEmail },
            { "cus_add1", "Dhaka" },
            { "cus_city", "Dhaka" },
            { "cus_postcode", "1219" },
            { "cus_country", "Bangladesh" },
            { "cus_phone", "+880" },
            { "shipping_method", "NO" },
            { "product_name", "Test Product" },
            { "product_category", "Service" },
            { "product_profile", "general" }
        };

            var url = await _ssl.CreateSessionAsync(payload, ct);

            if (string.IsNullOrEmpty(url))
                return BadRequest(new PaymentSessionResponse());

            return Ok(new PaymentSessionResponse
            {
                Status = "SUCCESS",
                GatewayUrl = url
            });
        }

        /// Step 2: Browser returns here after payment
        [HttpPost("success")]
        public async Task<IActionResult> PaymentSuccess([FromForm] string val_id, [FromForm] string tran_id, CancellationToken ct)
        {
            var validation = await _ssl.ValidateAsync(val_id, ct);
            if (validation is null) return BadRequest();

            if (!int.TryParse(tran_id, out var paymentId))
                return BadRequest(new { error = "Invalid payment id" });

            var payment = await _context.Payments
                .Include(p=>p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

            if (payment == null) return NotFound(new { error = "Payment not found" });

            payment.PaymentStatus = Models.PaymentStatus.Success;
            payment.Booking.IsCompleted = true;
            await _context.SaveChangesAsync(ct);

            return Ok(new { status = "success", transaction = validation.RootElement });
        }

        [HttpPost("fail")]
        public IActionResult PaymentFail() => Ok(new { status = "fail" });

        [HttpPost("cancel")]
        public IActionResult PaymentCancel() => Ok(new { status = "cancel" });

        /// Step 3: IPN server-to-server
        [HttpPost("ipn")]
        public async Task<IActionResult> PaymentIpn([FromForm] string val_id, CancellationToken ct)
        {
            var validation = await _ssl.ValidateAsync(val_id, ct);

            if (validation is null) return BadRequest();

            return Ok(new { status = "ipn_received", data = validation.RootElement });
        }


    }
}
