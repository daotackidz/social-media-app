using Microsoft.AspNetCore.Http;

namespace Social.Service.Social.File.Interface
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<bool> DeleteFileAsync(string containerName, string fileName);
    }
}
