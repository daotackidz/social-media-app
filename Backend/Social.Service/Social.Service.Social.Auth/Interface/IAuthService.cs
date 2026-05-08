using Social.Data.Model.Request.Login;

namespace Social.Service.Social.Auth.Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message)> VerifyEmailAsync(string email, string otpCode);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);
    }
}
