using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Repository;
using Social.Repository.Social.Story.Interface;

namespace Social.Repository.Social.Story.Repository
{
    public class StoryRepository : IStoryRepository
    {
        private readonly SocialDbContext _db;

        public StoryRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task<List<StoryFeedGroup>> GetFeedPageAsync(IEnumerable<Guid> ownerUserIds, Guid viewerUserId, int skip, int take)
        {
            var ids = ownerUserIds.ToList();
            if (ids.Count == 0) return new List<StoryFeedGroup>();

            var now = DateTime.UtcNow;

            var grouped = _db.Stories
                .Where(s => ids.Contains(s.UserId) && s.RecordStatusId == RecordStatus.Status.Active && s.ExpiresDate > now)
                .GroupBy(s => s.UserId)
                .Select(g => new StoryFeedGroup
                {
                    UserId = g.Key,
                    LatestCreatedDate = g.Max(s => s.CreatedDate),
                    Viewed = g.All(s => s.StoryViews!.Any(v => v.ViewerUserId == viewerUserId))
                });

            return await grouped
                .OrderBy(g => g.Viewed)
                .ThenByDescending(g => g.LatestCreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
