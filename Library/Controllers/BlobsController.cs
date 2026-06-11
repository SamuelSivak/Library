using Library.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobsController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public BlobsController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            using var stream = file.OpenReadStream();
            var fileId = await _blobService.UploadFileAsync(stream, file.FileName, file.ContentType);
            
            var fileUrl = $"{Request.Scheme}://{Request.Host}/api/blobs/{fileId}";
            return Ok(new { Url = fileUrl, FileId = fileId });
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> Get(string fileId)
        {
            try
            {
                var (stream, contentType) = await _blobService.DownloadFileAsync(fileId);
                return File(stream, contentType);
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("File not found in storage.");
            }
            catch (System.Exception)
            {
                return NotFound("File not found.");
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(string fileId)
        {
            var result = await _blobService.DeleteFileAsync(fileId);
            if (!result)
                return NotFound("Failed to delete file.");

            return Ok(new { Message = "File deleted successfully." });
        }
    }
}
