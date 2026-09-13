using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Story
{
    /// <summary>Likes on a story — same soft-delete/reactivate shape as PostLikes, scoped to a story instead of a post.</summary>
    [Table("story_likes")]
    public class StoryLikes : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("story_id", Order = 1)]
        public Guid StoryId { get; set; }

        [Column("user_id", Order = 2)]
        public Guid UserId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(StoryId))]
        public Stories? Stories { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
