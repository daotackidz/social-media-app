using Social.Common.Constants;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;

namespace Social.Service.Social.Auth.Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, RegisterResponse? Data, ErrorCode? Code)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message, UserResponse? Data, ErrorCode? Code)> VerifyEmailAsync(VerifyEmailRequest request);
        Task<(bool Success, string Message, ErrorCode? Code)> ResendOtpAsync(string email);
        Task<(bool Success, string Message, ForgotPasswordResponse? Data, ErrorCode? Code)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string Message, ErrorCode? Code)> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request);
        Task<(bool Success, string Message, ErrorCode? Code)> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
