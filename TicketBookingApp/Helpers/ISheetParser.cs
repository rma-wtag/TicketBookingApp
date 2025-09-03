using TicketBookingApp.Dtos.ShowDtos;

namespace TicketBookingApp.Helpers
{
    public interface ISheetParser<T>
    {
        Task<List<T>> ParseCsvAsync(IFormFile file);
        Task<List<T>> ParseExcelAsync(IFormFile file);
    }
}
