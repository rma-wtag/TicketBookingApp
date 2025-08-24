namespace TicketBookingApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string Name {  get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string ClientURL { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
