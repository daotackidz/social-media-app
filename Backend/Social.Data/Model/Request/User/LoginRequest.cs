using Social.Common.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.User
{
    public class LoginRequest
    {
        [Display(Name = "Email", Prompt = "abc@xyz...")]
        [EmailAddress(ErrorMessage = "{0} không đúng định dạng")]
        [StringLength(maximumLength: SocialConstantsLengths.Length100, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [MaxLength(SocialConstantsLengths.Length200, ErrorMessage = "{0} quá dài")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public required string Email { get; set; }

        [Display(Name = "Mật khẩu", Prompt = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [PasswordPropertyText]
        [StringLength(maximumLength: SocialConstantsLengths.Length50, MinimumLength = SocialConstantsLengths.Length2, ErrorMessage = "{0} cần có độ dài từ {2} đến {1} ký tự")]
        [DataType(DataType.Password)]
        [MaxLength(SocialConstantsLengths.Length50, ErrorMessage = "{0} quá dài")]
        public required string Password { get; set; }
    }
}
