using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Post
{
    public class CreatePostRequest
    {
        public string? Caption { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; } = new();

        /// <summary>Người đăng đánh dấu nội dung có sử dụng/được tạo bởi AI (nhãn "AI info").</summary>
        public bool IsAiGenerated { get; set; }

        /// <summary>Mô tả thay thế (accessibility) cho từng tệp, cùng thứ tự với <see cref="Files"/>.</summary>
        public List<string?> AltTexts { get; set; } = new();
    }
}
