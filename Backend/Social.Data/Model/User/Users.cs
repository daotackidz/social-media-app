using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("users")]
    [Index(nameof(HashText), nameof(UserName), nameof(PassWord), IsUnique = false)]
    [Index(nameof(Email), IsUnique = false)]
    [Index(nameof(PhoneNumber), IsUnique = false)]
    public class Users : BaseCatalog
    {
        public class UserIdConst
        {
            public const int DefaultSystem = SocialConstantValue.DefaultSystemId;
            public const int Guest = 1;
            public const int Dev = 2;
            public const int Admin = 3;
            public const int User = 4;
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_name", Order = 1)]
        [Display(Name = "Tên đăng nhập", Prompt = "abc,...")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public required string UserName { get; set; }

        [Column("password", Order = 2)]
        [Display(Name = "Mật khẩu", Prompt = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [PasswordPropertyText]
        [StringLength(maximumLength: SocialConstantsLengths.Length50, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [DataType(DataType.Password)]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public required string PassWord { get; set; } = SocialConstant.DefaultPassword;

        [Column("salt_password", Order = 2)]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        public string SaltPassWord { get; set; }

        [Column("user_locked", Order = 4)]
        [Display(Name = "Khóa tài khoản")]
        [ScaffoldColumn(false)]
        public bool UserLocked { get; set; } = false;

        [Column("phone_number_comfirmed", Order = 5)]
        [Display(Name = "Đã comfirm số điện thoại")]
        [ScaffoldColumn(false)]
        public bool PhoneNumberConfirm { get; set; } = false;

        [Column("email_comfirmed", Order = 6)]
        [Display(Name = "Đã comfirm email")]
        [ScaffoldColumn(false)]
        public bool EmailConfirmed { get; set; } = false;

        [Column("hash_text", Order = 7)]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length0, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length1000, ErrorMessage = "{0} quá dài")]
        public string HashText { get; set; }

        [Column("key_reset_password", Order = 8)]
        [Display(Name = "Mã reset mật khẩu")]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        public string KeyResetPassWord { get; set; }

        [Column("key_reset_password_expiration_date_unix", Order = 9)]
        [Display(Name = "Thời gian hết hiệu lực mã reset mật khẩu")]
        public long? KeyResetPassWordExpirationDateUnix { get; set; } = 0;

        [Column("token", Order = 9)]
        [Display(Name = "Token", Prompt = "Token")]
        [StringLength(maximumLength: SocialConstantsLengths.Length500, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length500, ErrorMessage = "{0} quá dài")]
        public string Token { get; set; }

        [Column("token_expiration_date_unix", Order = 10)]
        [Display(Name = "Thời gian hết hiệu lực token")]
        public long? TokenExpirationDateUnix { get; set; } = 0;

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
        public string Otp { get; set; }

        [Column("otp_expiration_date_unix")]
        public long? OtpExpirationDateUnix { get; set; } = 0;

        #region Related Tables

        #endregion
    }
}
