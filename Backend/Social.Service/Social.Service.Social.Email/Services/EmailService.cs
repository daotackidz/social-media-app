using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using MimeKit;
using Social.Service.Social.Email.Interface;
using Social.Service.Social.Email.Models;

namespace Social.Service.Social.Email.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;
        private readonly OtpSettings _otpSettings;

        public EmailService(IOptions<EmailSettings> settings,
            ILogger<EmailService> logger,
            IOptions<OtpSettings> otpSettings)
        {
            _settings = settings.Value;
            _logger = logger;
            _otpSettings = otpSettings.Value;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Mã xác nhận đăng ký tài khoản";

            message.Body = new TextPart("html")
            {
                Text = $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                    <h2 style="color: #333;">Xác nhận đăng ký tài khoản</h2>
                    <p>Mã xác nhận của bạn là:</p>
                    <div style="font-size: 36px; font-weight: bold; letter-spacing: 8px;
                                color: #4F46E5; padding: 20px; text-align: center;
                                background: #F3F4F6; border-radius: 8px;">
                        {otpCode}
                    </div>
                    <p style="color: #666; margin-top: 16px;">
                        Mã có hiệu lực trong <strong>{_otpSettings.ExpiryMinutes} phút</strong>.<br/>
                        Không chia sẻ mã này với bất kỳ ai.
                    </p>
                </div>
            """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("OTP sent to {Email}", toEmail);
        }

        // Services/Implement/EmailService.cs
        public async Task SendForgotPasswordOtpAsync(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Mã xác nhận đặt lại mật khẩu";

            message.Body = new TextPart("html")
            {
                Text = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                <h2 style="color: #333;">Đặt lại mật khẩu</h2>
                <p>Bạn vừa yêu cầu đặt lại mật khẩu. Mã xác nhận của bạn là:</p>
                <div style="font-size: 36px; font-weight: bold; letter-spacing: 8px;
                            color: #E53E3E; padding: 20px; text-align: center;
                            background: #FFF5F5; border-radius: 8px;">
                    {otpCode}
                </div>
                <p style="color: #666; margin-top: 16px;">
                    Mã có hiệu lực trong <strong>{_otpSettings.ExpiryMinutes} phút</strong>.<br/>
                    Nếu bạn không yêu cầu, hãy bỏ qua email này.
                </p>
            </div>
        """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
