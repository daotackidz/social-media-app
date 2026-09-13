using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Story
{
    public class CreateStoryRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        public string? Caption { get; set; }

        /// <summary>Người đăng đánh dấu nội dung có sử dụng/được tạo bởi AI (nhãn "AI info").</summary>
        public bool IsAiGenerated { get; set; }
    }
}
