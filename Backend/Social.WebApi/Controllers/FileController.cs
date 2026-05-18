using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Request.File;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;

namespace Social.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : BaseApiController
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            try
            {

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { message = "File is required." });
                }

                if (string.IsNullOrWhiteSpace(request.ContainerName))
                {
                    return BadRequest(new { message = "Container name is required." });
                }

                var url = await _fileService.UploadFileAsync(request.File, request.ContainerName);
                return Ok(new { url });
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"[Azure Error] Status: {ex.Status}, Code: {ex.ErrorCode}, Message: {ex.Message}");
                throw;
            }
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> Delete([FromQuery] string containerName, [FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "Container name and file name are required." });
            }

            var deleted = await _fileService.DeleteFileAsync(containerName, fileName);
            if (!deleted)
            {
                return NotFound(new { message = "File not found or already deleted." });
            }

            return Ok(new { message = "File deleted successfully." });
        }
    }
}
