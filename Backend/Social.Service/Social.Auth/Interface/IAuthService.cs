namespace Social.Service.Social.Auth.Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(string email, string password);
        Task<(bool Success, string Message)> VerifyEmailAsync(string email, string otpCode);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);
    }
}
