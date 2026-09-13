using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.Story;
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

        private static bool IsActiveAndUnexpired(Stories s, DateTime now)
            => s.RecordStatusId == RecordStatus.Status.Active && s.ExpiresDate > now;

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

        public async Task CreateAsync(Stories story)
        {
            await _db.Stories.AddAsync(story);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Stories>> GetActiveByUserIdAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _db.Stories
                .Include(s => s.Files)
                .Where(s => s.UserId == userId && s.RecordStatusId == RecordStatus.Status.Active && s.ExpiresDate > now)
                .OrderBy(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<Stories?> GetByIdAsync(Guid storyId)
        {
            // No expiry filter here — see the interface doc: an expired story is still valid, just
            // restricted to its owner by the controller.
            return await _db.Stories
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.Id == storyId && s.RecordStatusId == RecordStatus.Status.Active);
        }

        public async Task<List<Stories>> GetArchivedByUserIdAsync(Guid userId, int skip, int take)
            => await _db.Stories
                .Include(s => s.Files)
                .Where(s => s.UserId == userId && s.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(s => s.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<bool> MarkViewedAsync(Guid storyId, Guid viewerUserId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == storyId && s.RecordStatusId == RecordStatus.Status.Active);
                if (story is null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                var alreadyViewed = await _db.StoryViews.AnyAsync(v => v.StoryId == storyId && v.ViewerUserId == viewerUserId);
                if (!alreadyViewed)
                {
                    await _db.StoryViews.AddAsync(new StoryViews
                    {
                        Id = Guid.NewGuid(),
                        StoryId = storyId,
                        ViewerUserId = viewerUserId,
                        CreatedByUserId = viewerUserId
                    });
                    story.ViewCount += 1;
                    _db.Stories.Update(story);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(bool Liked, int LikeCount)?> ToggleLikeAsync(Guid storyId, Guid userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == storyId && s.RecordStatusId == RecordStatus.Status.Active);
                if (story is null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // Never hard-deleted, same as post/comment likes — a repeat like/unlike reactivates
                // or soft-deletes the same row instead of piling up duplicates.
                var existing = await _db.StoryLikes.FirstOrDefaultAsync(l => l.StoryId == storyId && l.UserId == userId);
                bool liked;

                if (existing is null)
                {
                    await _db.StoryLikes.AddAsync(new StoryLikes
                    {
                        Id = Guid.NewGuid(),
                        StoryId = storyId,
                        UserId = userId,
                        CreatedByUserId = userId
                    });
                    story.LikeCount += 1;
                    liked = true;
                }
                else if (existing.RecordStatusId == RecordStatus.Status.Active)
                {
                    existing.RecordStatusId = RecordStatus.Status.Deleted;
                    _db.StoryLikes.Update(existing);
                    story.LikeCount = Math.Max(0, story.LikeCount - 1);
                    liked = false;
                }
                else
                {
                    existing.RecordStatusId = RecordStatus.Status.Active;
                    _db.StoryLikes.Update(existing);
                    story.LikeCount += 1;
                    liked = true;
                }

                _db.Stories.Update(story);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (liked, story.LikeCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<HashSet<Guid>> GetLikedStoryIdsAsync(IEnumerable<Guid> storyIds, Guid userId)
        {
            var ids = storyIds.ToList();
            if (ids.Count == 0) return new HashSet<Guid>();

            var liked = await _db.StoryLikes
                .Where(l => ids.Contains(l.StoryId) && l.UserId == userId && l.RecordStatusId == RecordStatus.Status.Active)
                .Select(l => l.StoryId)
                .ToListAsync();

            return liked.ToHashSet();
        }

        public async Task<StoryReplies> AddReplyAsync(Guid storyId, Guid userId, string content)
        {
            var reply = new StoryReplies
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                UserId = userId,
                Content = content,
                CreatedByUserId = userId
            };

            await _db.StoryReplies.AddAsync(reply);
            await _db.SaveChangesAsync();
            return reply;
        }

        public async Task<List<StoryViewerInfo>> GetViewersAsync(Guid storyId, int skip, int take)
        {
            var likedUserIds = await _db.StoryLikes
                .Where(l => l.StoryId == storyId && l.RecordStatusId == RecordStatus.Status.Active)
                .Select(l => l.UserId)
                .ToListAsync();
            var likedSet = likedUserIds.ToHashSet();

            var views = await _db.StoryViews
                .Where(v => v.StoryId == storyId)
                .OrderByDescending(v => v.CreatedDate)
                .Skip(skip)
                .Take(take)
                .Select(v => new { v.ViewerUserId, v.CreatedDate })
                .ToListAsync();

            return views
                .Select(v => new StoryViewerInfo { UserId = v.ViewerUserId, ViewedDate = v.CreatedDate, Liked = likedSet.Contains(v.ViewerUserId) })
                .ToList();
        }
    }
}
