using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.User
{
    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required] string FullName,
        [Required] string Username,
        [Required] DateTime DateOfBirth
    );

    public record VerifyEmailRequest(
        [Required, EmailAddress] string Email,
        [Required] string OtpCode
    );

    public record ResendOtpRequest(
        [Required, EmailAddress] string Email
    );
}