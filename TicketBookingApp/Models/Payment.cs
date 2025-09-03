using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketBookingApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        [Required(ErrorMessage = "Payment amount is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        
    }

    public enum PaymentStatus { 
        Pending,
        Processing,
        Success,
        Failed,
    }
}
