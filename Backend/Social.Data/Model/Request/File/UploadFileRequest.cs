using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.File
{
    public class UploadFileRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        [Required]
        public string ContainerName { get; set; } = string.Empty;
    }
}
