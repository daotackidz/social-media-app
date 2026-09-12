namespace Social.Data.Model.Response.Feed
{
    /// <summary>One ring in the stories tray — one row per owner (grouped), not per story.</summary>
    public class FeedStoryResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        /// <summary>True when the current user has already viewed every active story this owner has.</summary>
        public bool Viewed { get; set; }
    }
}
