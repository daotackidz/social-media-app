using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;

namespace Social.Service.Social.Auth.Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, RegisterResponse? Data)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message, UserResponse? Data)> VerifyEmailAsync(VerifyEmailRequest request);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);
        Task<(bool Success, string Message, ForgotPasswordResponse? Data)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
