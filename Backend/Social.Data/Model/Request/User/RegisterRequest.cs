using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.User
{
    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required] string FullName,
        [Required, RegularExpression("^[a-zA-Z0-9._]{3,30}$", ErrorMessage = "Tên người dùng chỉ gồm chữ, số, dấu chấm và gạch dưới, từ 3-30 ký tự.")] string Username,
        [Required] DateTime? DateOfBirth
    );

    public record VerifyEmailRequest(
        [Required, EmailAddress] string Email,
        [Required] string OtpCode
    );

    public record ResendOtpRequest(
        [Required, EmailAddress] string Email
    );

    public class UpdateUserProfileRequest
    {
        public IFormFile? AvatarFile { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public ushort? Gender { get; set; }
        public string? Address { get; set; }
    }
}