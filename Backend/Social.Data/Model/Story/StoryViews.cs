using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Story
{
    /// <summary>One record per (story, viewer) — drives the viewed/unviewed ring state in the tray.</summary>
    [Table("story_views")]
    public class StoryViews : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("story_id", Order = 1)]
        public Guid StoryId { get; set; }

        [Column("viewer_user_id", Order = 2)]
        public Guid ViewerUserId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(StoryId))]
        public Stories? Stories { get; set; }

        [ForeignKey(nameof(ViewerUserId))]
        public Users? Viewer { get; set; }

        #endregion
    }
}
