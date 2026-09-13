using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Story
{
    /// <summary>
    /// A text reply sent to a story's owner (the "Trả lời ..." box in the story viewer). There's
    /// no direct-message inbox in this app yet, so replies are simply recorded against the story
    /// for the owner to read — insert-only, never edited or soft-deleted.
    /// </summary>
    [Table("story_replies")]
    public class StoryReplies : BaseRecordModel
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

        [Column("content", Order = 3)]
        public string Content { get; set; } = string.Empty;

        #region Related Tables

        [ForeignKey(nameof(StoryId))]
        public Stories? Stories { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
