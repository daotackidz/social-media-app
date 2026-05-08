using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Social.EmailServices.Interface;
using Social.EmailServices.Models;

namespace Social.EmailServices.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
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
                        Mã có hiệu lực trong <strong>10 phút</strong>.<br/>
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
    }
}
