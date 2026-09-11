using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Post
{
    public class CreatePostRequest
    {
        public string? Caption { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; } = new();
    }
}
