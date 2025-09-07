namespace TicketBookingApp.AzureServices
{
    public interface IAzureBlobService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
