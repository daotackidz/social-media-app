using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.Post;
using Social.Data.Repository;
using Social.Repository.Social.Post.Interface;

namespace Social.Repository.Social.Post.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly SocialDbContext _db;

        public PostRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Posts post, List<PostFiles> postFiles)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Posts.AddAsync(post);
                await _db.PostFiles.AddRangeAsync(postFiles);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
            => await _db.Posts
                .Where(p => p.UserId == userId && p.RecordStatusId == RecordStatus.Status.Active)
                .CountAsync();

        public async Task<List<Posts>> GetByUserIdAsync(Guid userId, int skip, int take)
            => await _db.Posts
                .Include(p => p.PostFiles!)
                    .ThenInclude(f => f.Files)
                .Where(p => p.UserId == userId && p.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<List<Posts>> GetTaggedByUserIdAsync(Guid userId, int skip, int take)
        {
            var taggedPostIds = _db.PostTags
                .Where(t => t.UserId == userId && t.RecordStatusId == RecordStatus.Status.Active)
                .Select(t => t.PostId);

            return await _db.Posts
                .Include(p => p.PostFiles!)
                    .ThenInclude(f => f.Files)
                .Where(p => taggedPostIds.Contains(p.Id) && p.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Posts>> GetFeedPageAsync(IEnumerable<Guid> ownerUserIds, int skip, int take)
        {
            var ids = ownerUserIds.ToList();
            if (ids.Count == 0) return new List<Posts>();

            return await _db.Posts
                .Include(p => p.PostFiles!)
                    .ThenInclude(f => f.Files)
                .Where(p => ids.Contains(p.UserId) && p.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
