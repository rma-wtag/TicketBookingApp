namespace TicketBookingApp.Models.PaymentGwModels
{
    public class PaymentSessionResponse
    {
        public string Status { get; set; } = "FAILED";
        public string GatewayUrl { get; set; } = "";
    }
}
