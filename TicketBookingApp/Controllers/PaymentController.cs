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
            { "tran_id", req.BookingId.ToString() },

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

            if (!int.TryParse(tran_id, out var BookingId))
                return BadRequest(new { error = "Invalid booking id" });

            var booking = await _context.Bookings
                                        .Include(b => b.Payment)
                                        .FirstOrDefaultAsync(b => b.Id == BookingId, ct);

            if (booking == null) return NotFound(new { error = "Booking not found" });

            // Update database
            booking.Payment.PaymentStatus = Models.PaymentStatus.Success;
            booking.IsCompleted = true;
            await _context.SaveChangesAsync(ct);

            //Redirect to Blazor front-end page
            return Redirect($"https://localhost:7143/payment/success?tran_id={tran_id}");
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
