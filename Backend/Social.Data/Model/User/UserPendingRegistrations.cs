using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("user_pending_registrations")]
    public class UserPendingRegistrations : BaseRecordModel
    {
        [Key]
        [Column("id")]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        [Display(Name = "PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("user_name")]
        [Display(Name = "Tên người dùng", Prompt = "abc,...")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public string? UserName { get; set; }

        [Column("otp_code")]
        [Display(Name = "OtpCode")]
        public string OtpCode { get; set; } = string.Empty;

        [Column("expires_at")]
        [Display(Name = "ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_verified")]
        [Display(Name = "IsVerified")]
        public bool IsVerified { get; set; }

        [Column("date_of_birth", Order = 5)]
        [Display(Name = "Ngày sinh")]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public DateTime? DateOfBirth { get; set; }

        [Column("full_name", Order = 2)]
        [Display(Name = "Họ và tên", Prompt = "Họ và tên")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public string? FullName { get; set; }

        #region Related Tables

        #endregion
    }
}
