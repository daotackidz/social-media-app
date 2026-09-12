namespace Social.Data.Model.Response.Feed
{
    /// <summary>One "Suggested for you" card/row.</summary>
    public class FeedSuggestedUserResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>"followsYou" | "followedBy" | "new" — the frontend picks the caption from this.</summary>
        public string Reason { get; set; } = "new";

        /// <summary>Username of the mutual connection, set only when Reason is "followedBy".</summary>
        public string? ReasonUsername { get; set; }
    }
}
