namespace Social.Repository.Social.Story.Interface
{
    /// <summary>One tray ring's worth of story data — the repository only knows story rows, not the owner's display info (that's IUserRepository's job).</summary>
    public class StoryFeedGroup
    {
        public Guid UserId { get; set; }
        public DateTime LatestCreatedDate { get; set; }
        public bool Viewed { get; set; }
    }

    public interface IStoryRepository
    {
        /// <summary>
        /// One row per owner (ownerUserIds) with at least one active, unexpired story —
        /// unviewed owners first, then newest first. Fetches take+1 so the caller can
        /// tell whether another page exists without a separate count query.
        /// </summary>
        Task<List<StoryFeedGroup>> GetFeedPageAsync(IEnumerable<Guid> ownerUserIds, Guid viewerUserId, int skip, int take);
    }
}
