namespace Social.EmailServices.Models
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
    }

    public class OtpSettings
    {
        public int ExpiryMinutes { get; set; } = 10;
        public int Length { get; set; } = 6;
    }
}