using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;

namespace TicketBookingApp.AzureServices
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly BlobContainerClient _blobClient;
        public AzureBlobService(BlobServiceClient blobServiceClient)
        {
            var containerName = "movie-posters";
            _blobClient = blobServiceClient.GetBlobContainerClient(containerName);
            _blobClient.CreateIfNotExists(PublicAccessType.Blob);
        }
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) {
                throw new ArgumentException("File is empty");
            }

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            var fileName = fileNameWithoutExt + ".webp";

            var blobClient = _blobClient.GetBlobClient(fileName);

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            using var outputStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = 75
            };
            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            await blobClient.UploadAsync(outputStream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}
