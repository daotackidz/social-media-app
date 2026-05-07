using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("user_profile")]
    public class UserProfiles : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        [Column("full_name", Order = 2)]
        [Display(Name = "Họ và tên", Prompt = "Họ và tên")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public string FullName { get; set; }

        [Column("first_name", Order = 3)]
        [Display(Name = "Họ")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public string FirstName { get; set; }

        [Column("last_name", Order = 4)]
        [Display(Name = "Tên")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public string LastName { get; set; }

        [Column("date_of_birth", Order = 5)]
        [Display(Name = "Ngày sinh")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public DateTime DateOfBirth { get; set; }

        [Column("gender", Order = 6)]
        [Display(Name = "Giới tính")]
        public ushort Gender { get; set; }

        [Column("address")]
        [Display(Name = "Địa chỉ")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public string Address { get; set; }

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
