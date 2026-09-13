namespace Social.Data.Model.Response.Feed
{
    /// <summary>One card in the home feed — the read side of the infinite-scroll post list.</summary>
    public class FeedPostResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Verified { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>Every image attached to the post, in display order — the primary/cover image first. Empty for a video post.</summary>
        public List<string> ImageUrls { get; set; } = new();

        /// <summary>Set when the post is a video — a post is either an image/carousel or a single video, never both.</summary>
        public string? VideoUrl { get; set; }

        /// <summary>The video's auto-captured cover frame — for &lt;video poster&gt;, so it isn't a blank/black box before playback starts. Only set alongside VideoUrl.</summary>
        public string? PosterUrl { get; set; }
        public string Caption { get; set; } = string.Empty;
        public int LikeCount { get; set; }

        /// <summary>True when the caller has an active like on this post.</summary>
        public bool IsLiked { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
