namespace Social.Data.Model.Response.Feed
{
    /// <summary>One card in the home feed — the read side of the infinite-scroll post list.</summary>
    public class FeedPostResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Verified { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>Every image attached to the post, in display order — the primary/cover image first.</summary>
        public List<string> ImageUrls { get; set; } = new();
        public string Caption { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
