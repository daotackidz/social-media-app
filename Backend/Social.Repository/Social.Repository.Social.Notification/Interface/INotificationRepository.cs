using Social.Data.Model.Notification;

namespace Social.Repository.Social.Notification.Interface
{
    public interface INotificationRepository
    {
        /// <summary>
        /// For toggle-style events (post/story likes): reactivates and re-timestamps an existing
        /// notification for the same (UserId, ActorUserId, Type, PostId, StoryId) instead of piling
        /// up a new row every time the same person unlikes/relikes; inserts a fresh one otherwise.
        /// Callers only call this for the "liked" side of the toggle, never for "unliked".
        /// </summary>
        Task UpsertLikeNotificationAsync(Notifications notification);

        /// <summary>One-time events (comments/replies, follow, follow request, follow accepted) — always inserted as a new row.</summary>
        Task CreateAsync(Notifications notification);

        /// <summary>Newest first, paged. Fetches take+1 so the caller can tell whether another page exists without a separate count query.</summary>
        Task<List<Notifications>> GetPageAsync(Guid userId, int skip, int take);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task MarkAllReadAsync(Guid userId);

        /// <summary>Returns false if the notification doesn't exist or doesn't belong to userId.</summary>
        Task<bool> MarkReadAsync(Guid notificationId, Guid userId);
    }
}
