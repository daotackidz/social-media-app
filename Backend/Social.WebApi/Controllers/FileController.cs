using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
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
                    return ApiBadRequest("File là bắt buộc.", ErrorCode.FILE_REQUIRED);
                }

                if (string.IsNullOrWhiteSpace(request.ContainerName))
                {
                    return ApiBadRequest("Tên container là bắt buộc.", ErrorCode.CONTAINER_REQUIRED);
                }

                var url = await _fileService.UploadFileAsync(request.File, request.ContainerName);
                return ApiOk(new { url }, "Tải file lên thành công.");
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
                return ApiBadRequest("Tên container và tên file là bắt buộc.", ErrorCode.CONTAINER_REQUIRED);
            }

            var deleted = await _fileService.DeleteFileAsync(containerName, fileName);
            if (!deleted)
            {
                return ApiNotFound("Không tìm thấy file hoặc đã bị xoá.", ErrorCode.FILE_NOT_FOUND);
            }

            return ApiOk<object>(null!, "Xoá file thành công.");
        }
    }
}
