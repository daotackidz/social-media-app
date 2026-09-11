using Social.Common.Constants;
using Social.Data.Model.Base;
using Social.Data.Model.File;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    /// <summary>
    /// A "story highlight" circle shown on a profile page (grouped saved stories
    /// under a cover + title). The individual stories inside a highlight aren't
    /// modeled yet — there is no Stories table in this schema — so this only
    /// covers what the profile page needs to render the highlight row.
    /// </summary>
    [Table("user_highlights")]
    public class UserHighlights : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        [Column("title", Order = 2)]
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public string Title { get; set; } = string.Empty;

        [Column("cover_file_id", Order = 3)]
        public Guid? CoverFileId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        [ForeignKey(nameof(CoverFileId))]
        public Files? CoverFile { get; set; }

        #endregion
    }
}
