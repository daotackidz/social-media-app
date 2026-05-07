using Social.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Base
{
    [Table("record_status")]
    public class RecordStatus : BaseModel
    {
        public enum Status
        {
            SystemUsing = 0,
            Active = 1,
            Deactive = 2,
            Deleted = 3,
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Status Id { get; set; }

        [Column("status_name", Order = 1)]
        [Display(Name = "Tên trạng thái", Prompt = "abc,...")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public required string StatusName { get; set; }
    }
}
