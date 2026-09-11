namespace Social.Data.Model.Response.Profile
{
    public class ProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? AvatarUrl { get; set; }
        public bool Verified { get; set; }

        /// <summary>True when the caller (JWT) is viewing their own profile.</summary>
        public bool IsCurrentUser { get; set; }

        /// <summary>True when the caller already follows this profile. Always false when IsCurrentUser.</summary>
        public bool IsFollowing { get; set; }

        /// <summary>Username of someone the caller follows who also follows this profile, e.g. "Followed by X".</summary>
        public string? FollowedByUsername { get; set; }

        public int PostsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }

    public class ProfileHighlightResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
    }

    public class ProfilePostResponse
    {
        public Guid Id { get; set; }

        /// <summary>"image" | "video" | "carousel" — matches the frontend's ProfilePostType.</summary>
        public string Type { get; set; } = "image";
        public string? CoverUrl { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
