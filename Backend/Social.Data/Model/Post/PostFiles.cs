using Social.Common.Constants;
using Social.Data.Model.Base;
using Social.Data.Model.File;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    [Table("post_files")]
    public class PostFiles : BaseRecordModel
    {
        public enum PostFileType
        {
            Image = 1,
            Video = 2,
            Document = 3,
            Attachment = 4
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("post_id", Order = 1)]
        public Guid PostId { get; set; }

        [Column("file_id", Order = 2)]
        public Guid FileId { get; set; }

        [Column("file_type", Order = 3)]
        public PostFileType FileType { get; set; }

        [Column("is_primary", Order = 4)]
        public bool IsPrimary { get; set; }

        [Column("alt_text")]
        [Display(Name = "Mô tả thay thế")]
        public string? AltText { get; set; }

        #region Related Tables

        [ForeignKey(nameof(PostId))]
        public Posts? Posts { get; set; }

        [ForeignKey(nameof(FileId))]
        public Files? Files { get; set; }

        #endregion
    }
}
