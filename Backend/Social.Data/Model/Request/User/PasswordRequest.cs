using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.User
{
    public record ForgotPasswordRequest(
        [Required, EmailAddress] string Email
    );

    public record ResetPasswordRequest(
        [Required, EmailAddress] string Email,
        [Required] string OtpCode,
        [Required, MinLength(8)] string NewPassword,
        [Required] string ConfirmPassword
    );
}