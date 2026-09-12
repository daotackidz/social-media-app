using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.User;
using Social.Data.Repository;
using Social.Repository.Social.Relation.Interface;

namespace Social.Repository.Social.Relation.Repository
{
    public class UserRelationRepository : IUserRelationRepository
    {
        private readonly SocialDbContext _db;

        public UserRelationRepository(SocialDbContext db)
        {
            _db = db;
        }

        private IQueryable<UserRelations> ActiveFollows()
            => _db.UserRelations.Where(r =>
                r.RelationType == UserRelations.UserRelationType.Follow
                && r.Status == UserRelations.UserRelationStatus.Accepted
                && r.RecordStatusId == RecordStatus.Status.Active);

        /// <summary>Same as ActiveFollows() but also includes Pending rows — for the operations that must
        /// treat "already requested" and "already following" as one existing relation (follow/unfollow).</summary>
        private IQueryable<UserRelations> ActiveFollowsOrRequests()
            => _db.UserRelations.Where(r =>
                r.RelationType == UserRelations.UserRelationType.Follow
                && (r.Status == UserRelations.UserRelationStatus.Accepted || r.Status == UserRelations.UserRelationStatus.Pending)
                && r.RecordStatusId == RecordStatus.Status.Active);

        public async Task<int> CountFollowersAsync(Guid userId)
            => await ActiveFollows().CountAsync(r => r.FollowingUserId == userId);

        public async Task<int> CountFollowingAsync(Guid userId)
            => await ActiveFollows().CountAsync(r => r.FollowerUserId == userId);

        public async Task<bool> IsFollowingAsync(Guid followerUserId, Guid followingUserId)
            => await ActiveFollows().AnyAsync(r => r.FollowerUserId == followerUserId && r.FollowingUserId == followingUserId);

        public async Task<UserRelations.UserRelationStatus?> GetRelationStatusAsync(Guid followerUserId, Guid followingUserId)
            => await ActiveFollowsOrRequests()
                .Where(r => r.FollowerUserId == followerUserId && r.FollowingUserId == followingUserId)
                .Select(r => (UserRelations.UserRelationStatus?)r.Status)
                .FirstOrDefaultAsync();

        public async Task<UserRelations.UserRelationStatus> FollowAsync(Guid followerUserId, Guid followingUserId, bool requiresApproval)
        {
            var targetStatus = requiresApproval ? UserRelations.UserRelationStatus.Pending : UserRelations.UserRelationStatus.Accepted;
            if (followerUserId == followingUserId) return targetStatus;

            // Includes Rejected rows too, so a past reject can be re-requested instead of erroring on a duplicate key.
            var relation = await _db.UserRelations.FirstOrDefaultAsync(r =>
                r.RelationType == UserRelations.UserRelationType.Follow
                && r.RecordStatusId == RecordStatus.Status.Active
                && r.FollowerUserId == followerUserId && r.FollowingUserId == followingUserId);

            if (relation is not null)
            {
                if (relation.Status is UserRelations.UserRelationStatus.Accepted or UserRelations.UserRelationStatus.Pending)
                    return relation.Status;

                relation.Status = targetStatus;
                _db.UserRelations.Update(relation);
                await _db.SaveChangesAsync();
                return targetStatus;
            }

            await _db.UserRelations.AddAsync(new UserRelations
            {
                Id = Guid.NewGuid(),
                FollowerUserId = followerUserId,
                FollowingUserId = followingUserId,
                RelationType = UserRelations.UserRelationType.Follow,
                Status = targetStatus,
                CreatedByUserId = followerUserId
            });
            await _db.SaveChangesAsync();
            return targetStatus;
        }

        public async Task UnfollowAsync(Guid followerUserId, Guid followingUserId)
        {
            var relation = await ActiveFollowsOrRequests()
                .FirstOrDefaultAsync(r => r.FollowerUserId == followerUserId && r.FollowingUserId == followingUserId);
            if (relation is null) return;

            relation.RecordStatusId = RecordStatus.Status.Deleted;
            _db.UserRelations.Update(relation);
            await _db.SaveChangesAsync();
        }

        public async Task<List<PendingFollowRequest>> GetPendingFollowRequestsAsync(Guid targetUserId)
            => await _db.UserRelations
                .Where(r => r.RelationType == UserRelations.UserRelationType.Follow
                         && r.Status == UserRelations.UserRelationStatus.Pending
                         && r.RecordStatusId == RecordStatus.Status.Active
                         && r.FollowingUserId == targetUserId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new PendingFollowRequest { RelationId = r.Id, FollowerUserId = r.FollowerUserId, CreatedDate = r.CreatedDate })
                .ToListAsync();

        public async Task<Guid?> ApproveFollowRequestAsync(Guid relationId, Guid targetUserId)
        {
            var relation = await _db.UserRelations.FirstOrDefaultAsync(r =>
                r.Id == relationId
                && r.FollowingUserId == targetUserId
                && r.Status == UserRelations.UserRelationStatus.Pending
                && r.RecordStatusId == RecordStatus.Status.Active);
            if (relation is null) return null;

            relation.Status = UserRelations.UserRelationStatus.Accepted;
            _db.UserRelations.Update(relation);
            await _db.SaveChangesAsync();
            return relation.FollowerUserId;
        }

        public async Task<bool> RejectFollowRequestAsync(Guid relationId, Guid targetUserId)
        {
            var relation = await _db.UserRelations.FirstOrDefaultAsync(r =>
                r.Id == relationId
                && r.FollowingUserId == targetUserId
                && r.Status == UserRelations.UserRelationStatus.Pending
                && r.RecordStatusId == RecordStatus.Status.Active);
            if (relation is null) return false;

            relation.Status = UserRelations.UserRelationStatus.Rejected;
            _db.UserRelations.Update(relation);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetFollowedByUsernameAsync(Guid? currentUserId, Guid targetUserId)
        {
            if (!currentUserId.HasValue || currentUserId.Value == targetUserId) return null;

            var followingOfCurrentUser = ActiveFollows()
                .Where(r => r.FollowerUserId == currentUserId.Value)
                .Select(r => r.FollowingUserId);

            var mutualFollowerId = await ActiveFollows()
                .Where(r => r.FollowingUserId == targetUserId && followingOfCurrentUser.Contains(r.FollowerUserId))
                .Select(r => r.FollowerUserId)
                .FirstOrDefaultAsync();

            if (mutualFollowerId == Guid.Empty) return null;

            return await _db.Users
                .Where(u => u.Id == mutualFollowerId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Guid>> GetFollowingUserIdsAsync(Guid userId)
            => await ActiveFollows()
                .Where(r => r.FollowerUserId == userId)
                .Select(r => r.FollowingUserId)
                .ToListAsync();

        public async Task<List<SuggestionCandidate>> GetSuggestionsAsync(Guid userId, int limit)
        {
            var followingIds = await GetFollowingUserIdsAsync(userId);
            var excludeIds = new HashSet<Guid>(followingIds) { userId };
            var results = new List<SuggestionCandidate>();

            // A) People who follow me that I don't follow back.
            var followsYou = await ActiveFollows()
                .Where(r => r.FollowingUserId == userId && !excludeIds.Contains(r.FollowerUserId))
                .Select(r => r.FollowerUserId)
                .Distinct()
                .Take(limit)
                .ToListAsync();

            results.AddRange(followsYou.Select(id => new SuggestionCandidate { UserId = id, Reason = "followsYou" }));
            foreach (var id in followsYou) excludeIds.Add(id);

            // B) Followed by someone I follow (2nd degree) — grouped/deduped in memory since
            // the row count here is bounded by follow-graph size, not the whole users table.
            if (results.Count < limit && followingIds.Count > 0)
            {
                var secondDegreeRows = await ActiveFollows()
                    .Where(r => followingIds.Contains(r.FollowerUserId) && !excludeIds.Contains(r.FollowingUserId))
                    .Select(r => new { r.FollowingUserId, r.FollowerUserId })
                    .ToListAsync();

                var secondDegree = secondDegreeRows
                    .GroupBy(r => r.FollowingUserId)
                    .Select(g => new SuggestionCandidate { UserId = g.Key, Reason = "followedBy", ReasonUserId = g.First().FollowerUserId })
                    .Take(limit - results.Count)
                    .ToList();

                results.AddRange(secondDegree);
                foreach (var s in secondDegree) excludeIds.Add(s.UserId);
            }

            // C) Fallback filler: other active accounts, newest first.
            if (results.Count < limit)
            {
                var fillers = await _db.Users
                    .Where(u => u.RecordStatusId == RecordStatus.Status.Active && !excludeIds.Contains(u.Id))
                    .OrderByDescending(u => u.CreatedDate)
                    .Select(u => u.Id)
                    .Take(limit - results.Count)
                    .ToListAsync();

                results.AddRange(fillers.Select(id => new SuggestionCandidate { UserId = id, Reason = "new" }));
            }

            return results;
        }
    }
}
