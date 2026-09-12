namespace Social.Data.Model.Response.User
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? WebsiteUrl { get; set; }
        public ushort? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>True khi tài khoản riêng tư — yêu cầu theo dõi mới phải chờ duyệt.</summary>
        public bool IsPrivate { get; set; }
    }
}
