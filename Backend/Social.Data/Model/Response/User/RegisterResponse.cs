namespace Social.Data.Model.Response.User
{
    public class RegisterResponse
    {
        public string Email { get; set; } = string.Empty;
        public int OtpExpiresInSeconds { get; set; }
    }
}
