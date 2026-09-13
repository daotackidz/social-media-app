using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    /// <summary>Likes on a comment (or reply) — same soft-delete/reactivate shape as PostLikes, just scoped to a comment instead of a post.</summary>
    [Table("post_comment_likes")]
    public class PostCommentLikes : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("comment_id", Order = 1)]
        public Guid CommentId { get; set; }

        [Column("user_id", Order = 2)]
        public Guid UserId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(CommentId))]
        public PostComments? PostComments { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
