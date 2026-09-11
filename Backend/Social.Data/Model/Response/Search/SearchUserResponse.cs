namespace Social.Data.Model.Response.Search
{
    /// <summary>One user row shown in the search popup — a live search hit or a recent-history entry.</summary>
    public class SearchUserResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool Verified { get; set; }
        public bool IsFollowing { get; set; }
    }

    /// <summary>A "Recent" row — the history entry id plus the target user's public info.</summary>
    public class SearchHistoryResponse
    {
        public Guid Id { get; set; }
        public SearchUserResponse User { get; set; } = new();
    }
}
