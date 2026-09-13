using Social.Data.Model.Notification;

namespace Social.Data.Model.Response.Notification
{
    public class NotificationActorResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// One row of the notifications popup. Likes on the same post/story from several people are
    /// grouped into a single row (Actors holds the most recent few, TotalActorCount the full tally) —
    /// every other type is always one actor per row.
    /// </summary>
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public Notifications.NotificationType Type { get; set; }
        public List<NotificationActorResponse> Actors { get; set; } = new();
        public int TotalActorCount { get; set; }

        /// <summary>Short preview of the comment/reply text — only set for CommentPost/CommentStory.</summary>
        public string? CommentPreview { get; set; }

        public Guid? PostId { get; set; }
        public Guid? StoryId { get; set; }

        /// <summary>Small thumbnail shown at the end of the row — the post's cover, or the story's own media.</summary>
        public string? ThumbnailUrl { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>Only set for a single-actor "Follow" row — whether the caller already follows this actor back, to drive the Follow/Following button.</summary>
        public bool? IsFollowingActor { get; set; }
    }
}
