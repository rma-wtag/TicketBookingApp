using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using TicketBookingApp.Entities;
using TicketBookingApp.Services;

namespace TicketBookingApp.Repositories
{
    public class UnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        public MovieRepository MovieRepository { get; set; }
        public HallRepository HallRepository { get; set; }
        public ShowRepository ShowRepository { get; set; }
        public PaymentRepository PaymentRepository { get; set; }

        public UnitOfWork(ApplicationDbContext context, IDistributedCache cache, ILogService logService)
        {
            _context = context;
            _cache = cache;
            _logService = logService;
            MovieRepository = new MovieRepository(context,cache, logService);
            HallRepository = new HallRepository(context,cache, logService);
            ShowRepository = new ShowRepository(context, cache, logService);
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
