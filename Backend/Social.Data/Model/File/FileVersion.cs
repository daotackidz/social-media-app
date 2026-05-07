using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.File
{
    [Table("file_version")]
    public class FileVersion : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("file_id", Order = 1)]
        public Guid FileId { get; set; }

        [Column("file_name", Order = 2)]
        [Display(Name = "Tên tập tin")]
        public string FileName { get; set; }

        [Column("file_name_origin", Order = 3)]
        [Display(Name = "Tên tập tin gốc")]
        public string FileNameOrigin { get; set; }

        [Column("file_extension", Order = 4)]
        [Display(Name = "Đuôi file")]
        public string FileExtension { get; set; }

        [Column("file_type", Order = 5)]
        [Display(Name = "Loại file")]
        public ushort FileType { get; set; }

        [Column("file_size")]
        [Display(Name = "Kích thước")]
        public long FileSize { get; set; }

        [Column("storage_path")]
        [Display(Name = "Đường dẫn lưu trữ")]
        public string StoragePath { get; set; }

        #region Related Tables

        [ForeignKey(nameof(FileId))]
        public Files? Files { get; set; }

        #endregion
    }
}
