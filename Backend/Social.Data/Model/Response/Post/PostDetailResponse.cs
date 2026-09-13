namespace Social.Data.Model.Response.Post
{
    /// <summary>
    /// The "open post" overlay — same media/caption/counts as a feed card plus
    /// the comment thread, requested on demand (clicking a thumbnail or the
    /// comment icon) rather than carried on every feed/grid item.
    /// </summary>
    public class PostDetailResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Verified { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? VideoUrl { get; set; }

        /// <summary>The video's auto-captured cover frame — for &lt;video poster&gt;. Only set alongside VideoUrl.</summary>
        public string? PosterUrl { get; set; }
        public string Caption { get; set; } = string.Empty;
        public bool Edited { get; set; }
        public int LikeCount { get; set; }

        /// <summary>True when the caller has an active like on this post.</summary>
        public bool IsLiked { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>First page only (10 top-level comments) — the rest is paged in on demand via GET .../comments.</summary>
        public List<PostCommentResponse> Comments { get; set; } = new();

        /// <summary>Whether more top-level comments exist beyond the page in Comments.</summary>
        public bool CommentsHasMore { get; set; }
    }

    /// <summary>Result of toggling a like — the new state plus the post's updated count, so the client doesn't need a second round-trip.</summary>
    public class PostLikeResponse
    {
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }

    /// <summary>Result of toggling a like on a comment (or reply) — same shape as PostLikeResponse, kept distinct since it's the comment's own count, not the post's.</summary>
    public class CommentLikeResponse
    {
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }

    public class PostCommentResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int LikeCount { get; set; }

        /// <summary>True when the caller has an active like on this comment.</summary>
        public bool IsLiked { get; set; }

        /// <summary>Number of replies — 0, and not meaningful, on a reply itself (replies are flat, never nested further).</summary>
        public int ReplyCount { get; set; }
    }

    /// <summary>One row of the "Lượt thích" (likes list) popup.</summary>
    public class PostLikeUserResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>True for the row belonging to the caller themselves — the client hides the Follow button for it.</summary>
        public bool IsCurrentUser { get; set; }

        /// <summary>Whether the caller currently follows this liker — drives the Follow/Following button state.</summary>
        public bool IsFollowing { get; set; }
    }
}
