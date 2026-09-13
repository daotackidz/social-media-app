using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Post
{
    public class AddCommentRequest
    {
        [Required(ErrorMessage = "Bình luận không được để trống.")]
        [StringLength(2200, ErrorMessage = "Bình luận quá dài.")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// When replying, the comment being replied to. Replies are kept flat (one level) like
        /// Instagram's — replying to a reply is re-parented to that reply's own top-level comment
        /// server-side, so a thread never nests more than one level deep.
        /// </summary>
        public Guid? ParentId { get; set; }
    }
}
