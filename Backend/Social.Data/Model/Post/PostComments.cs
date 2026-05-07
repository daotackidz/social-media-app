using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    [Table("post_comments")]
    public class PostComments : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("post_id", Order = 1)]
        public Guid PostId { get; set; }

        [Column("user_id", Order = 2)]
        public Guid UserId { get; set; }

        [Column("content", Order = 3)]
        public string Content { get; set; }

        [Column("parent_id", Order = 4)]
        public Guid? ParentID { get; set; }

        [Column("path", Order = 5)]
        public string Path { get; set; }

        [Column("depth")]
        public int Depth { get; set; }

        [Column("like_count")]
        public int LikeCount { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        #region Related Tables

        [ForeignKey(nameof(PostId))]
        public Posts? Posts { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
