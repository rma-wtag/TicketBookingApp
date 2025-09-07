
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TicketBookingApp.AzureServices
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly BlobContainerClient _blobClient;
        public AzureBlobService(BlobServiceClient blobServiceClient)
        {
            var containerName = "movie-posters";
            _blobClient = blobServiceClient.GetBlobContainerClient(containerName);
            _blobClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) {
                throw new ArgumentException("File is empty");
            }

            var blobClient = _blobClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }
    }
}
