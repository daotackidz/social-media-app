using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    [Table("post_tags")]
    public class PostTags : BaseRecordModel
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

        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        #region Related Tables

        [ForeignKey(nameof(PostId))]
        public Posts? Posts { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
