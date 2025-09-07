using Microsoft.EntityFrameworkCore.Storage;
using TicketBookingApp.Entities;

namespace TicketBookingApp.Repositories
{
    public class UnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        public MovieRepository MovieRepository { get; set; }
        public HallRepository HallRepository { get; set; }
        public ShowRepository ShowRepository { get; set; }
        public PaymentRepository PaymentRepository { get; set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            MovieRepository = new MovieRepository(context);
            HallRepository = new HallRepository(context);
            ShowRepository = new ShowRepository(context);
            PaymentRepository = new PaymentRepository(context);
            _transaction = _context.Database.BeginTransaction();
        }
        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }
        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
