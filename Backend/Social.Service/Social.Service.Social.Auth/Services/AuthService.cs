using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Generators;
using Social.Common.Constants;
using Social.Common.Handlers;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;
using Social.Data.Model.User;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.Auth.Interface;
using Social.Service.Social.Email.Interface;
using Social.Service.Social.Email.Models;
using System.Security.Cryptography;
using static Social.Data.Model.User.UserPendingRegistrations;

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

        public async Task<(bool Success, string Message, RegisterResponse? Data, ErrorCode? Code)> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.ToLowerInvariant().Trim();
            var username = request.Username.Trim().ToLowerInvariant();

            if (await _userRepository.EmailExistsAsync(email))
                return (false, "Email đã được sử dụng.", null, ErrorCode.EMAIL_EXISTS);

            if (await _userRepository.UsernameExistsAsync(username))
                return (false, "Tên người dùng đã được sử dụng.", null, ErrorCode.USERNAME_EXISTS);

            // Xóa các request cũ chưa xác nhận
            await _pendingRepository.DeleteOldAsync(email, PendingOtpType.Register);

            var otp = GenerateOtp();

            var pending = new UserPendingRegistrations
            {
                Email = email,
                PasswordHash = PasswordHashHandler.HashPassWord(request.Password),
                FullName = request.FullName.Trim(),
                DateOfBirth = request.DateOfBirth,
                OtpCode = otp,
                UserName = username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes),
                Type = PendingOtpType.Register,
            };

            await _pendingRepository.AddAsync(pending);
            await _emailService.SendOtpEmailAsync(email, otp);

            return (true, $"Mã xác nhận đã được gửi tới {email}.", new RegisterResponse
            {
                Email = pending.Email,
                OtpExpiresInSeconds = _otpSettings.ExpiryMinutes * 60
            }, null);
        }

        public async Task<(bool Success, string Message, UserResponse? Data, ErrorCode? Code)> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var email = request.Email.ToLowerInvariant().Trim();

            var pending = await _pendingRepository.GetLatestAsync(email, PendingOtpType.Register);

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.", null, ErrorCode.PENDING_REGISTRATION_NOT_FOUND);

            if (DateTime.UtcNow > pending.ExpiresAt)
                return (false, "Mã xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại.", null, ErrorCode.OTP_EXPIRED);

            if (pending.OtpCode != request.OtpCode.Trim())
                return (false, "Mã xác nhận không đúng.", null, ErrorCode.OTP_INVALID);

            var username = pending.UserName ?? pending.Email;

            // Ai đó có thể đã đăng ký/xác thực cùng username này trong lúc chờ OTP.
            if (await _userRepository.UsernameExistsAsync(username))
                return (false, "Tên người dùng đã được sử dụng.", null, ErrorCode.USERNAME_EXISTS);

            // Tạo User và UserProfile từ dữ liệu đã lưu tạm
            var user = new Users
            {
                Id = Guid.NewGuid(),
                UserName = username,
                Email = email,
                PasswordHash = pending.PasswordHash,
                EmailVerified = true,
            };

            user.CreatedByUserId = user.Id; // Người tạo là chính họ

            var profile = new UserProfiles
            {
                Id = Guid.NewGuid(),
                FullName = pending.FullName ?? "",
                DateOfBirth = pending.DateOfBirth,
                CreatedByUserId = user.Id,
            };

            await _userRepository.CreateUserAsync(user, profile);

            // Đánh dấu đã xác nhận
            pending.IsVerified = true;
            await _pendingRepository.UpdateAsync(pending);

            return (true, "Đăng ký tài khoản thành công!", new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = profile.FullName,
                DateOfBirth = profile.DateOfBirth,
                CreatedDate = user.CreatedDate
            }, null);
        }

        public async Task<(bool Success, string Message, ErrorCode? Code)> ResendOtpAsync(string email)
        {
            email = email.ToLowerInvariant().Trim();

            if (await _userRepository.EmailExistsAsync(email))
                return (false, "Email này đã có tài khoản.", ErrorCode.EMAIL_EXISTS);

            var pending = await _pendingRepository.GetLatestAsync(email, PendingOtpType.Register);

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.", ErrorCode.PENDING_REGISTRATION_NOT_FOUND);

            if ((DateTime.UtcNow - pending.CreatedDate).TotalSeconds < 60)
                return (false, "Vui lòng đợi 1 phút trước khi gửi lại mã.", ErrorCode.OTP_COOLDOWN);

            pending.OtpCode = GenerateOtp();
            pending.ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);
            pending.CreatedDate = DateTime.UtcNow;
            pending.CreatedDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _pendingRepository.UpdateAsync(pending);
            await _emailService.SendOtpEmailAsync(email, pending.OtpCode);

            return (true, "Mã xác nhận mới đã được gửi.", null);
        }

        public async Task<(bool Success, string Message, ForgotPasswordResponse? Data, ErrorCode? Code)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var email = request.Email.ToLowerInvariant().Trim();

            if (!await _userRepository.EmailExistsAsync(email))
                return (false, "Email không tồn tại trong hệ thống.", null, ErrorCode.EMAIL_NOT_FOUND);

            var existing = await _pendingRepository.GetLatestAsync(email, PendingOtpType.ForgotPassword);
            if (existing is not null && (DateTime.UtcNow - existing.CreatedDate).TotalSeconds < 60)
                return (false, "Vui lòng đợi 1 phút trước khi gửi lại mã.", null, ErrorCode.OTP_COOLDOWN);

            await _pendingRepository.DeleteOldAsync(email, PendingOtpType.ForgotPassword);

            var otp = GenerateOtp();

            // ForgotPassword không cần FullName, DateOfBirth, PasswordHash
            await _pendingRepository.AddAsync(new UserPendingRegistrations
            {
                Email = email,
                OtpCode = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes),
                Type = PendingOtpType.ForgotPassword
            });

            await _emailService.SendForgotPasswordOtpAsync(email, otp);

            return (true, $"Mã xác nhận đã được gửi tới {email}.", new ForgotPasswordResponse
            {
                Email = email,
                OtpExpiresInSeconds = _otpSettings.ExpiryMinutes * 60
            }, null);
        }

        public async Task<(bool Success, string Message, ErrorCode? Code)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var email = request.Email.ToLowerInvariant().Trim();

            if (request.NewPassword != request.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.", ErrorCode.PASSWORD_MISMATCH);

            var pending = await _pendingRepository.GetLatestAsync(email, PendingOtpType.ForgotPassword);

            if (pending is null || DateTime.UtcNow > pending.ExpiresAt)
                return (false, "Mã xác nhận đã hết hạn hoặc không tồn tại.", ErrorCode.OTP_EXPIRED);

            if (pending.OtpCode != request.OtpCode.Trim())
                return (false, "Mã xác nhận không đúng.", ErrorCode.OTP_INVALID);

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return (false, "Không tìm thấy tài khoản.", ErrorCode.USER_NOT_FOUND);

            user.PasswordHash = PasswordHashHandler.HashPassWord(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            pending.IsVerified = true;
            await _pendingRepository.UpdateAsync(pending);

            return (true, "Đổi mật khẩu thành công.", null);
        }

        #region Private method

        private string GenerateOtp()
            => RandomNumberGenerator
                .GetInt32(100_000, 999_999)
                .ToString();

        #endregion
    }
}
