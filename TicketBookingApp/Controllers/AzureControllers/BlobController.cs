using Microsoft.AspNetCore.Mvc;
using TicketBookingApp.AzureServices;

namespace TicketBookingApp.Controllers.AzureControllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BlobController : ControllerBase
    {
        private readonly IAzureBlobService _blobService;
        public BlobController(IAzureBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file) {
            var url = await _blobService.UploadFileAsync(file);
            return Ok(url);
        }
    }
}
