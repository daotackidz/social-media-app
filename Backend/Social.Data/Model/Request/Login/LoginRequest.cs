using Social.Common.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Login
{
    public class LoginRequest
    {
        [Display(Name = "Tên người dùng", Prompt = "abc,...")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length100, ErrorMessage = "{0} quá dài")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu", Prompt = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [PasswordPropertyText]
        [StringLength(maximumLength: SocialConstantsLengths.Length50, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [DataType(DataType.Password)]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public string Password { get; set; }
    }
}
