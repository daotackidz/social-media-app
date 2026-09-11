using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    [Table("posts")]
    public class Posts : BaseRecordModel
    {
        public enum PostPrivacy
        {
            Private = 1,
            Followers = 2,
            Public = 3
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        [Column("caption", Order = 2)]
        [Display(Name = "Tiêu đề")]
        public string Caption { get; set; }

        [Column("content", Order = 3)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Column("privacy")]
        [Display(Name = "Phạm vi")]
        public PostPrivacy Privacy { get; set; }

        [Column("like_count")]
        [Display(Name = "Lượt thích")]
        public int LikeCount { get; set; }

        [Column("comment_count")]
        [Display(Name = "Lượt bình luận")]
        public int CommentCount { get; set; }

        [Column("shared_count")]
        [Display(Name = "Lượt chia sẻ")]
        public int SharedCount { get; set; }

        [Column("is_edit")]
        [Display(Name = "Đã chỉnh sửa")]
        public bool IsEdited { get; set; }

        [Column("is_delete")]
        [Display(Name = "Đã xoá")]
        public bool IsDeleted { get; set; }

        [Column("original_post_id")]
        [Display(Name = "Bài viết gốc")]
        public Guid? OriginalPostId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        [ForeignKey(nameof(OriginalPostId))]
        public Posts? OriginalPost { get; set; }

        public ICollection<PostFiles>? PostFiles { get; set; }

        #endregion
    }
}
