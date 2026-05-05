using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.File
{
    [Table("files")]
    public class Files : BaseCatalog
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("file_name", Order = 1)]
        [Display(Name = "Tên tập tin")]
        public string FileName { get; set; }

        [Column("file_name_origin", Order = 2)]
        [Display(Name = "Tên tập tin gốc")]
        public string FileNameOrigin { get; set; }

        [Column("file_extension", Order = 3)]
        [Display(Name = "Đuôi file")]
        public string FileExtension { get; set; }

        [Column("file_type", Order = 4)]
        [Display(Name = "Loại file")]
        public ushort FileType { get; set; }

        [Column("file_size")]
        [Display(Name = "Kích thước")]
        public long FileSize { get; set; }

        [Column("storage_path")]
        [Display(Name = "Đường dẫn lưu trữ")]
        public string StoragePath { get; set; }

        [Column("storage_type")]
        [Display(Name = "Loại lưu trữ file")]
        public ushort StorageType { get; set; }

        [Column("is_public")]
        [Display(Name = "Loại công khai hay riêng tư")]
        public bool IsPublic { get; set; }

        [Column("file_version")]
        [Display(Name = "Version của tập tin")]
        public bool FileVersion { get; set; }

        #region Related Tables

        #endregion
    }
}
