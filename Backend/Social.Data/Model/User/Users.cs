using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("users")]
    [Index(nameof(HashText), nameof(UserName), nameof(PasswordHash), IsUnique = false)]
    [Index(nameof(Email), IsUnique = false)]
    [Index(nameof(PhoneNumber), IsUnique = false)]
    [Index(nameof(UserName), IsUnique = true)]
    public class Users : BaseRecordModel
    {
        public enum UserType
        {
            DefaultSystem = SocialConstantValue.DefaultSystemId,
            Guest = 1,
            Dev = 2,
            Admin = 3,
            User = 4
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_name", Order = 1)]
        [Display(Name = "Tên người dùng", Prompt = "abc,...")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public required string UserName { get; set; }

        [Column("password_hash", Order = 2)]
        [Display(Name = "Mật khẩu", Prompt = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [PasswordPropertyText]
        [StringLength(maximumLength: SocialConstantsLengths.Length200, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [DataType(DataType.Password)]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        public required string PasswordHash { get; set; } = SocialConstant.DefaultPassword;

        [Column("salt_password", Order = 3)]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        public string? SaltPassWord { get; set; }

        [Column("user_locked", Order = 4)]
        [Display(Name = "Khóa tài khoản")]
        [ScaffoldColumn(false)]
        public bool UserLocked { get; set; } = false;

        [Column("phone_number_comfirmed", Order = 5)]
        [Display(Name = "Đã comfirm số điện thoại")]
        [ScaffoldColumn(false)]
        public bool PhoneNumberConfirm { get; set; } = false;

        [Column("email_verified", Order = 6)]
        [Display(Name = "Đã xác thực email")]
        [ScaffoldColumn(false)]
        public bool EmailVerified { get; set; } = false;

        [Column("hash_text", Order = 7)]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length0, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length1000, ErrorMessage = "{0} quá dài")]
        public string? HashText { get; set; }

        [Column("key_reset_password", Order = 8)]
        [Display(Name = "Mã reset mật khẩu")]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        public string? KeyResetPassWord { get; set; }

        [Column("key_reset_password_expiration_date_unix", Order = 9)]
        [Display(Name = "Thời gian hết hiệu lực mã reset mật khẩu")]
        public long? KeyResetPassWordExpirationDateUnix { get; set; } = 0;

        [Column("token", Order = 10)]
        [Display(Name = "Token", Prompt = "Token")]
        [StringLength(maximumLength: SocialConstantsLengths.Length500, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length500, ErrorMessage = "{0} quá dài")]
        public string? Token { get; set; }

        [Column("token_expiration_date_unix", Order = 11)]
        [Display(Name = "Thời gian hết hiệu lực token")]
        public long? TokenExpirationDateUnix { get; set; } = 0;

        [Column("user_type")]
        [Display(Name = "Loại tài khoản")]
        public UserType Type { get; set; } = UserType.Guest;

        [Column("is_verified")]
        [Display(Name = "Tài khoản đã xác thực")]
        public bool? IsVerified { get; set; } = false;

        [Column("email")]
        [Display(Name = "Email", Prompt = "abc@xyz...")]
        [EmailAddress(ErrorMessage = "{0} không đúng định dạng")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Email { get; set; }

        [Column("phone_number")]
        [Display(Name = "Số điện thoại", Prompt = "0912xxxxxx,..")]
        [Phone(ErrorMessage = "{0} không đúng định dạng")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public string? PhoneNumber { get; set; }

        [Column("otp")]
        [Display(Name = "Otp", Prompt = "Otp")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length500, ErrorMessage = "{0} quá dài")]
        public string? Otp { get; set; }

        [Column("otp_expiration_date_unix")]
        public long? OtpExpirationDateUnix { get; set; } = 0;

        #region Related Tables

        #endregion
    }
}
