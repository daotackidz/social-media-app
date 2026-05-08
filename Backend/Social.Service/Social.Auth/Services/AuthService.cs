using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Social.Auth.Interface;
using Social.Common.Handlers;
using Social.Data.Model.Base;
using Social.Data.Model.User;
using Social.Data.Repository;
using System.Security.Cryptography;
using Social.Email.Models;
using Social.Email.Interface;

namespace Social.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly SocialDbContext _db;
        private readonly IEmailService _emailService;
        private readonly OtpSettings _otpSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            SocialDbContext db,
            IEmailService emailService,
            IOptions<OtpSettings> otpSettings,
            ILogger<AuthService> logger)
        {
            _db = db;
            _emailService = emailService;
            _otpSettings = otpSettings.Value;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password)
        {
            email = email.ToLowerInvariant().Trim();

            // Kiểm tra email đã tồn tại
            bool emailExists = await _db
                .Users
                .AnyAsync(u => u.Email == email
                    && u.RecordStatusId == RecordStatus.Status.Active);
            if (emailExists)
                return (false, "Email đã được sử dụng.");

            // Xóa các OTP cũ chưa xác nhận của email này
            var oldPending = await _db.UserPendingRegistrations
                .Where(p => p.Email == email && !p.IsVerified
                    && p.RecordStatusId == RecordStatus.Status.Active)
                .ToListAsync();
            _db.UserPendingRegistrations.RemoveRange(oldPending);

            // Tạo OTP
            string otp = GenerateOtp(_otpSettings.Length);
            string passwordHash = PasswordHashHandler.HashPassWord(password);

            var pending = new UserPendingRegistrations
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHash,
                OtpCode = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes)
            };

            _db.UserPendingRegistrations.Add(pending);
            await _db.SaveChangesAsync();

            // Gửi email
            await _emailService.SendOtpEmailAsync(email, otp);

            return (true, $"Mã xác nhận đã được gửi tới {email}. Vui lòng kiểm tra hộp thư.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string otpCode)
        {
            email = email.ToLowerInvariant().Trim();

            var pending = await _db.UserPendingRegistrations
                .Where(p => p.Email == email && !p.IsVerified
                    && p.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(p => p.CreatedDateUnix)
                .FirstOrDefaultAsync();

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.");

            if (DateTime.UtcNow > pending.ExpiresAt)
                return (false, "Mã xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại.");

            if (pending.OtpCode != otpCode.Trim())
                return (false, "Mã xác nhận không đúng.");

            // Tạo tài khoản
            var user = new Users
            {
                UserName = ,
                Email = email,
                PasswordHash = pending.PasswordHash,
                EmailVerified = true
            };

            pending.IsVerified = true;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {Email} registered successfully", email);
            return (true, "Đăng ký tài khoản thành công!");
        }

        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            email = email.ToLowerInvariant().Trim();

            bool emailExists = await _db.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
                return (false, "Email này đã có tài khoản.");

            var pending = await _db.UserPendingRegistrations
                .Where(p => p.Email == email && !p.IsVerified
                    && p.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(p => p.CreatedDateUnix)
                .FirstOrDefaultAsync();

            if (pending is null)
                return (false, "Không tìm thấy yêu cầu đăng ký. Vui lòng đăng ký lại.");

            // Rate limit: không gửi lại nếu < 1 phút
            if ((DateTime.UtcNow - pending.CreatedDate).TotalSeconds < 60)
                return (false, "Vui lòng đợi 1 phút trước khi gửi lại mã.");

            // Cập nhật OTP mới
            pending.OtpCode = GenerateOtp(_otpSettings.Length);
            pending.ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);
            pending.CreatedDate = DateTime.UtcNow;
            pending.CreatedDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _db.SaveChangesAsync();
            await _emailService.SendOtpEmailAsync(email, pending.OtpCode);

            return (true, "Mã xác nhận mới đã được gửi.");
        }

        private static string GenerateOtp(int length)
        {
            return RandomNumberGenerator.GetInt32(
                (int)Math.Pow(10, length - 1),
                (int)Math.Pow(10, length)
            ).ToString();
        }
    }
}
