using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.User;
using Social.Data.Repository;
using Social.Repository.Social.Search.Interface;

namespace Social.Repository.Social.Search.Repository
{
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly SocialDbContext _db;

        public SearchHistoryRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserSearchHistories>> GetRecentAsync(Guid userId, int take)
            => await _db.UserSearchHistories
                .Include(h => h.TargetUsers)
                .Where(h => h.UserId == userId && h.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(h => h.CreatedDate)
                .Take(take)
                .ToListAsync();

        public async Task AddOrTouchAsync(Guid userId, Guid targetUserId)
        {
            var existing = await _db.UserSearchHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.TargetUserId == targetUserId);

            if (existing is not null)
            {
                existing.RecordStatusId = RecordStatus.Status.Active;
                existing.CreatedDate = DateTime.UtcNow;
                existing.CreatedDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _db.UserSearchHistories.Update(existing);
            }
            else
            {
                await _db.UserSearchHistories.AddAsync(new UserSearchHistories
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TargetUserId = targetUserId,
                    CreatedByUserId = userId
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteAsync(Guid userId, Guid id)
        {
            var history = await _db.UserSearchHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && h.RecordStatusId == RecordStatus.Status.Active);
            if (history is null) return false;

            history.RecordStatusId = RecordStatus.Status.Deleted;
            _db.UserSearchHistories.Update(history);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SoftDeleteAllAsync(Guid userId)
        {
            var histories = await _db.UserSearchHistories
                .Where(h => h.UserId == userId && h.RecordStatusId == RecordStatus.Status.Active)
                .ToListAsync();

            foreach (var h in histories)
                h.RecordStatusId = RecordStatus.Status.Deleted;

            _db.UserSearchHistories.UpdateRange(histories);
            await _db.SaveChangesAsync();
        }
    }
}
