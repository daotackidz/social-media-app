using Microsoft.Extensions.Options;
using Social.Common.Handlers;
using Social.Data.Model.Request.Login;
using Social.Data.Model.User;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.Auth.Interface;
using Social.Service.Social.Email.Interface;
using Social.Service.Social.Email.Models;
using System.Security.Cryptography;

namespace Social.Service.Social.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserPendingRepository _pendingRepository;
        private readonly IEmailService _emailService;
        private readonly OtpSettings _otpSettings;

        public AuthService(
            IUserRepository userRepository,
            IUserPendingRepository pendingRepository,
            IEmailService emailService,
            IOptions<OtpSettings> otpSettings)
        {
            _userRepository = userRepository;
            _pendingRepository = pendingRepository;
            _emailService = emailService;
            _otpSettings = otpSettings.Value;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.ToLowerInvariant().Trim();

            if (await _userRepository.EmailExistsAsync(email))
                return (false, "Email đã được sử dụng.");

            // Xóa các request cũ chưa xác nhận
            await _pendingRepository.DeleteOldAsync(email);

            var otp = GenerateOtp();

            var pending = new UserPendingRegistrations
            {
                Email = email,
                PasswordHash = PasswordHashHandler.HashPassWord(request.Password),
                FullName = request.FullName.Trim(),
                DateOfBirth = request.DateOfBirth,
                OtpCode = otp,
                UserName = request.FullName ?? request.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes)
            };

            await _pendingRepository.AddAsync(pending);
            await _emailService.SendOtpEmailAsync(email, otp);

            return (true, $"Mã xác nhận đã được gửi tới {email}.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string otpCode)
        {
            email = email.ToLowerInvariant().Trim();

            var pending = await _pendingRepository.GetLatestAsync(email);

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.");

            if (DateTime.UtcNow > pending.ExpiresAt)
                return (false, "Mã xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại.");

            if (pending.OtpCode != otpCode.Trim())
                return (false, "Mã xác nhận không đúng.");

            // Tạo User và UserProfile từ dữ liệu đã lưu tạm
            var user = new Users
            {
                Id = Guid.NewGuid(),
                UserName = pending.UserName,
                Email = email,
                PasswordHash = pending.PasswordHash,
                EmailVerified = true
            };

            var profile = new UserProfiles
            {
                FullName = pending.FullName ?? "",
                DateOfBirth = pending.DateOfBirth
            };

            await _userRepository.CreateUserAsync(user, profile);

            // Đánh dấu đã xác nhận
            pending.IsVerified = true;
            await _pendingRepository.UpdateAsync(pending);

            return (true, "Đăng ký tài khoản thành công!");
        }

        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            email = email.ToLowerInvariant().Trim();

            if (await _userRepository.EmailExistsAsync(email))
                return (false, "Email này đã có tài khoản.");

            var pending = await _pendingRepository.GetLatestAsync(email);

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.");

            if ((DateTime.UtcNow - pending.CreatedDate).TotalSeconds < 60)
                return (false, "Vui lòng đợi 1 phút trước khi gửi lại mã.");

            pending.OtpCode = GenerateOtp();
            pending.ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);
            pending.CreatedDate = DateTime.UtcNow;
            pending.CreatedDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _pendingRepository.UpdateAsync(pending);
            await _emailService.SendOtpEmailAsync(email, pending.OtpCode);

            return (true, "Mã xác nhận mới đã được gửi.");
        }

        private string GenerateOtp()
            => RandomNumberGenerator
                .GetInt32(100_000, 999_999)
                .ToString();
    }
}
