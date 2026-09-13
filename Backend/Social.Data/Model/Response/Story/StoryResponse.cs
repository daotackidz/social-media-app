namespace Social.Data.Model.Response.Story
{
    /// <summary>One playable story item — a slide in the story viewer's reel for one owner.</summary>
    public class StoryItemResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string MediaUrl { get; set; } = string.Empty;

        /// <summary>"image" | "video" — video playback is capped at 15s client-side, same as Instagram's own stories.</summary>
        public string MediaType { get; set; } = "image";
        public string Caption { get; set; } = string.Empty;
        public bool IsAiGenerated { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }

        /// <summary>True when the caller has an active like on this story.</summary>
        public bool IsLiked { get; set; }

        /// <summary>True when the caller is the story's owner — the client hides like/reply and shows the viewer list instead.</summary>
        public bool IsOwnStory { get; set; }
    }

    /// <summary>Result of toggling a like on a story.</summary>
    public class StoryLikeResponse
    {
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }

    public class StoryReplyResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>One row of a story's "viewers" list — visible only to the story's own owner.</summary>
    public class StoryViewerResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime ViewedDate { get; set; }

        /// <summary>Whether this viewer also liked the story — surfaced as a small heart next to their row, like Instagram's own viewer list.</summary>
        public bool Liked { get; set; }
    }
}
