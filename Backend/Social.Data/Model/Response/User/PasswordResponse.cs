using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Response.User
{
    public class ForgotPasswordResponse
    {
        public string? Email { get; set; }
        public int OtpExpiresInSeconds { get; set; }
    };
}