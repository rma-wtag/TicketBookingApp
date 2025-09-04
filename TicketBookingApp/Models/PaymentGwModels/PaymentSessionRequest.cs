using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketBookingApp.Models.PaymentGwModels
{
    public class PaymentSessionRequest
    {
        public int PaymentId { get; set; }
        [Required(ErrorMessage = "Customer Name is required.")]
        public string CusName { get; set; } = default!;
        [Required(ErrorMessage = "Customer Email is required.")]
        [EmailAddress(ErrorMessage = "Must be a valid email address.")]
        public string CusEmail { get; set; } = default!;
        [Required(ErrorMessage = "Payment amount is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
