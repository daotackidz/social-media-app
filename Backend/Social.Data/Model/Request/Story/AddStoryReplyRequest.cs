using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Story
{
    public class AddStoryReplyRequest
    {
        [Required(ErrorMessage = "Trả lời không được để trống.")]
        [StringLength(2200, ErrorMessage = "Trả lời quá dài.")]
        public string Content { get; set; } = string.Empty;
    }
}
