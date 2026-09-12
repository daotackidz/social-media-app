using Social.Data.Model.Base;
using Social.Data.Model.File;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Story
{
    /// <summary>
    /// One story item — kept as its own table (not a Posts row) since a story
    /// has a lifetime (ExpiresDate, default 24h) and a different read model
    /// (grouped-by-owner ring in the tray, viewed/unviewed) than a feed post.
    /// </summary>
    [Table("stories")]
    public class Stories : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        [Column("file_id", Order = 2)]
        public Guid FileId { get; set; }

        [Column("expires_date", Order = 3)]
        public DateTime ExpiresDate { get; set; } = DateTime.UtcNow.AddHours(24);

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        [ForeignKey(nameof(FileId))]
        public Files? Files { get; set; }

        public ICollection<StoryViews>? StoryViews { get; set; }

        #endregion
    }
}
