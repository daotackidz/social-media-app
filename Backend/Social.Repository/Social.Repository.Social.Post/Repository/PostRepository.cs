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

        public async Task<Posts?> GetByIdAsync(Guid postId)
            => await _db.Posts
                .Include(p => p.PostFiles!)
                    .ThenInclude(f => f.Files)
                .FirstOrDefaultAsync(p => p.Id == postId && p.RecordStatusId == RecordStatus.Status.Active);

        public async Task<List<PostComments>> GetTopLevelCommentsAsync(Guid postId, int skip, int take)
            => await _db.PostComments
                .Where(c => c.PostId == postId && c.ParentID == null && !c.IsDeleted && c.RecordStatusId == RecordStatus.Status.Active)
                .OrderBy(c => c.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<List<PostComments>> GetRepliesAsync(Guid parentCommentId, int skip, int take)
            => await _db.PostComments
                .Where(c => c.ParentID == parentCommentId && !c.IsDeleted && c.RecordStatusId == RecordStatus.Status.Active)
                .OrderBy(c => c.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<Dictionary<Guid, int>> GetReplyCountsAsync(IEnumerable<Guid> parentCommentIds)
        {
            var ids = parentCommentIds.ToList();
            if (ids.Count == 0) return new Dictionary<Guid, int>();

            return await _db.PostComments
                .Where(c => c.ParentID != null && ids.Contains(c.ParentID.Value) && !c.IsDeleted && c.RecordStatusId == RecordStatus.Status.Active)
                .GroupBy(c => c.ParentID!.Value)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ParentId, g => g.Count);
        }

        public async Task<PostComments?> GetCommentByIdAsync(Guid commentId)
            => await _db.PostComments
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted && c.RecordStatusId == RecordStatus.Status.Active);

        public async Task<PostComments> AddCommentAsync(Guid postId, Guid userId, string content, Guid? parentId)
        {
            var comment = new PostComments
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Content = content,
                ParentID = parentId,
                Path = parentId?.ToString() ?? string.Empty,
                Depth = parentId.HasValue ? 1 : 0,
                CreatedByUserId = userId
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.PostComments.AddAsync(comment);

                var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                if (post is not null)
                {
                    post.CommentCount += 1;
                    _db.Posts.Update(post);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return comment;
        }

        public async Task<(bool Liked, int LikeCount)?> ToggleCommentLikeAsync(Guid commentId, Guid userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var comment = await _db.PostComments.FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted && c.RecordStatusId == RecordStatus.Status.Active);
                if (comment is null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                var existing = await _db.PostCommentLikes.FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
                bool liked;

                if (existing is null)
                {
                    await _db.PostCommentLikes.AddAsync(new PostCommentLikes
                    {
                        Id = Guid.NewGuid(),
                        CommentId = commentId,
                        UserId = userId,
                        CreatedByUserId = userId
                    });
                    comment.LikeCount += 1;
                    liked = true;
                }
                else if (existing.RecordStatusId == RecordStatus.Status.Active)
                {
                    existing.RecordStatusId = RecordStatus.Status.Deleted;
                    _db.PostCommentLikes.Update(existing);
                    comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                    liked = false;
                }
                else
                {
                    existing.RecordStatusId = RecordStatus.Status.Active;
                    _db.PostCommentLikes.Update(existing);
                    comment.LikeCount += 1;
                    liked = true;
                }

                _db.PostComments.Update(comment);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (liked, comment.LikeCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<HashSet<Guid>> GetLikedCommentIdsAsync(IEnumerable<Guid> commentIds, Guid userId)
        {
            var ids = commentIds.ToList();
            if (ids.Count == 0) return new HashSet<Guid>();

            var liked = await _db.PostCommentLikes
                .Where(l => ids.Contains(l.CommentId) && l.UserId == userId && l.RecordStatusId == RecordStatus.Status.Active)
                .Select(l => l.CommentId)
                .ToListAsync();

            return liked.ToHashSet();
        }

        public async Task<List<Guid>> GetCommentLikerUserIdsAsync(Guid commentId, int skip, int take)
            => await _db.PostCommentLikes
                .Where(l => l.CommentId == commentId && l.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(l => l.CreatedDate)
                .Skip(skip)
                .Take(take)
                .Select(l => l.UserId)
                .ToListAsync();

        public async Task<(bool Liked, int LikeCount)?> ToggleLikeAsync(Guid postId, Guid userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.RecordStatusId == RecordStatus.Status.Active);
                if (post is null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // Never hard-deleted, same as UserRelations follow/unfollow — a repeat
                // like/unlike reactivates or soft-deletes the same row instead of piling up duplicates.
                var existing = await _db.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
                bool liked;

                if (existing is null)
                {
                    await _db.PostLikes.AddAsync(new PostLikes
                    {
                        Id = Guid.NewGuid(),
                        PostId = postId,
                        UserId = userId,
                        CreatedByUserId = userId
                    });
                    post.LikeCount += 1;
                    liked = true;
                }
                else if (existing.RecordStatusId == RecordStatus.Status.Active)
                {
                    existing.RecordStatusId = RecordStatus.Status.Deleted;
                    _db.PostLikes.Update(existing);
                    post.LikeCount = Math.Max(0, post.LikeCount - 1);
                    liked = false;
                }
                else
                {
                    existing.RecordStatusId = RecordStatus.Status.Active;
                    _db.PostLikes.Update(existing);
                    post.LikeCount += 1;
                    liked = true;
                }

                _db.Posts.Update(post);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (liked, post.LikeCount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsLikedByUserAsync(Guid postId, Guid userId)
            => await _db.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId && l.RecordStatusId == RecordStatus.Status.Active);

        public async Task<HashSet<Guid>> GetLikedPostIdsAsync(IEnumerable<Guid> postIds, Guid userId)
        {
            var ids = postIds.ToList();
            if (ids.Count == 0) return new HashSet<Guid>();

            var liked = await _db.PostLikes
                .Where(l => ids.Contains(l.PostId) && l.UserId == userId && l.RecordStatusId == RecordStatus.Status.Active)
                .Select(l => l.PostId)
                .ToListAsync();

            return liked.ToHashSet();
        }

        public async Task<int> GetLikeCountAsync(Guid postId)
            => await _db.PostLikes.CountAsync(l => l.PostId == postId && l.RecordStatusId == RecordStatus.Status.Active);

        public async Task<List<Guid>> GetLikerUserIdsAsync(Guid postId, int skip, int take)
            => await _db.PostLikes
                .Where(l => l.PostId == postId && l.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(l => l.CreatedDate)
                .Skip(skip)
                .Take(take)
                .Select(l => l.UserId)
                .ToListAsync();
    }
}
