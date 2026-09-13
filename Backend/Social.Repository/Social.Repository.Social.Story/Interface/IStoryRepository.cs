using Social.Data.Model.Story;

namespace Social.Repository.Social.Story.Interface
{
    /// <summary>One tray ring's worth of story data — the repository only knows story rows, not the owner's display info (that's IUserRepository's job).</summary>
    public class StoryFeedGroup
    {
        public Guid UserId { get; set; }
        public DateTime LatestCreatedDate { get; set; }
        public bool Viewed { get; set; }
    }

    /// <summary>One row of a story's viewer list — who viewed it, when, and whether they also liked it.</summary>
    public class StoryViewerInfo
    {
        public Guid UserId { get; set; }
        public DateTime ViewedDate { get; set; }
        public bool Liked { get; set; }
    }

    public interface IStoryRepository
    {
        /// <summary>
        /// One row per owner (ownerUserIds) with at least one active, unexpired story —
        /// unviewed owners first, then newest first. Fetches take+1 so the caller can
        /// tell whether another page exists without a separate count query.
        /// </summary>
        Task<List<StoryFeedGroup>> GetFeedPageAsync(IEnumerable<Guid> ownerUserIds, Guid viewerUserId, int skip, int take);

        Task CreateAsync(Stories story);

        /// <summary>All of one owner's active, unexpired stories, oldest first (the order the viewer plays them in).</summary>
        Task<List<Stories>> GetActiveByUserIdAsync(Guid userId);

        /// <summary>
        /// One story with its file loaded, or null if it doesn't exist / was deleted. Deliberately
        /// not filtered by expiry — an expired story is still a valid row (it lives on in its
        /// owner's archive), so the caller (StoriesController) is the one that decides whether an
        /// expired story may be shown, based on who's asking.
        /// </summary>
        Task<Stories?> GetByIdAsync(Guid storyId);

        /// <summary>
        /// Every story ever posted by userId, regardless of expiry, newest first — the "Kho lưu trữ"
        /// (archive) page, visible only to the owner themselves. Fetches take+1 so the caller can
        /// tell whether another page exists without a separate count query.
        /// </summary>
        Task<List<Stories>> GetArchivedByUserIdAsync(Guid userId, int skip, int take);

        /// <summary>
        /// Records that viewerUserId has seen storyId (idempotent — a repeat view doesn't add a
        /// second row or re-bump the counter) and bumps the story's denormalized ViewCount on the
        /// first view only. Returns false if the story doesn't exist.
        /// </summary>
        Task<bool> MarkViewedAsync(Guid storyId, Guid viewerUserId);

        /// <summary>
        /// Likes storyId for userId if not already liked, unlikes it if it is (same soft-delete/
        /// reactivate pattern as post likes). Bumps the story's denormalized LikeCount in the same
        /// transaction. Returns null if the story doesn't exist.
        /// </summary>
        Task<(bool Liked, int LikeCount)?> ToggleLikeAsync(Guid storyId, Guid userId);

        /// <summary>Of storyIds, which ones userId currently has an active like on — for hydrating a story reel's IsLiked flags in one query.</summary>
        Task<HashSet<Guid>> GetLikedStoryIdsAsync(IEnumerable<Guid> storyIds, Guid userId);

        /// <summary>Records a text reply to a story (there's no DM inbox yet — this is simply stored for the owner to read).</summary>
        Task<StoryReplies> AddReplyAsync(Guid storyId, Guid userId, string content);

        /// <summary>
        /// Everyone who has viewed storyId, newest view first, with whether each also liked it —
        /// backs the story owner's "who viewed / who liked" list. Fetches take+1 so the caller can
        /// tell whether another page exists without a separate count query.
        /// </summary>
        Task<List<StoryViewerInfo>> GetViewersAsync(Guid storyId, int skip, int take);
    }
}
