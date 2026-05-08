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

        [Column("otp_code")]
        [Display(Name = "OtpCode")]
        public string OtpCode { get; set; } = string.Empty;

        [Column("expires_at")]
        [Display(Name = "ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_verified")]
        [Display(Name = "IsVerified")]
        public bool IsVerified { get; set; }

        #region Related Tables

        #endregion
    }
}
