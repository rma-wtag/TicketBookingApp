using TicketBookingApp.Entities;

namespace TicketBookingApp.Services.PaymentServices
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
