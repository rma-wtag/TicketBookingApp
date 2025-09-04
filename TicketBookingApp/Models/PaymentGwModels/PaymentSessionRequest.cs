namespace TicketBookingApp.Models.PaymentGwModels
{
    public class PaymentSessionRequest
    {
        public string TranId { get; set; } = default!;
        public decimal Amount { get; set; }
        public string CusName { get; set; } = default!;
        public string CusEmail { get; set; } = default!;
        public string CusPhone { get; set; } = default!;
    }
}
