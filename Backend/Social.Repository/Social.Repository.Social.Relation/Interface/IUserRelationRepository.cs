using Social.Data.Model.User;

namespace Social.Repository.Social.Relation.Interface
{
    /// <summary>One suggested-user candidate — the repository only knows the follow graph, not the candidate's display info (that's IUserRepository's job).</summary>
    public class SuggestionCandidate
    {
        public Guid UserId { get; set; }

        /// <summary>"followsYou" | "followedBy" | "new".</summary>
        public string Reason { get; set; } = "new";

        /// <summary>Set only for "followedBy" — the mutual connection's user id, for the "Followed by X" caption.</summary>
        public Guid? ReasonUserId { get; set; }
    }

    /// <summary>One pending follow request waiting for the target user's approval.</summary>
    public class PendingFollowRequest
    {
        public Guid RelationId { get; set; }
        public Guid FollowerUserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public interface IUserRelationRepository
    {
        Task<int> CountFollowersAsync(Guid userId);
        Task<int> CountFollowingAsync(Guid userId);
        Task<bool> IsFollowingAsync(Guid followerUserId, Guid followingUserId);

        /// <summary>Pending or Accepted status of followerUserId -> followingUserId, or null when there is none (never requested, or a past request was rejected).</summary>
        Task<UserRelations.UserRelationStatus?> GetRelationStatusAsync(Guid followerUserId, Guid followingUserId);

        /// <summary>
        /// Creates (or reactivates a previously rejected) follow relation. Auto-approved
        /// (Accepted) unless requiresApproval is true, in which case it starts Pending and
        /// needs ApproveFollowRequestAsync. Idempotent: calling again while already
        /// Pending/Accepted just returns the current status. Returns the resulting status.
        /// </summary>
        Task<UserRelations.UserRelationStatus> FollowAsync(Guid followerUserId, Guid followingUserId, bool requiresApproval);

        /// <summary>Removes the relation regardless of whether it was Accepted (unfollow) or Pending (cancel the request).</summary>
        Task UnfollowAsync(Guid followerUserId, Guid followingUserId);

        /// <summary>Pending requests waiting on targetUserId's approval, newest first.</summary>
        Task<List<PendingFollowRequest>> GetPendingFollowRequestsAsync(Guid targetUserId);

        /// <summary>Accepts a pending request. Returns the follower's user id on success, null if the relation doesn't exist, isn't targetUserId's, or isn't Pending.</summary>
        Task<Guid?> ApproveFollowRequestAsync(Guid relationId, Guid targetUserId);

        /// <summary>Rejects a pending request (kept as Rejected, not deleted, so it isn't immediately re-suggested). Returns false when the relation doesn't exist, isn't targetUserId's, or isn't Pending.</summary>
        Task<bool> RejectFollowRequestAsync(Guid relationId, Guid targetUserId);

        /// <summary>Username of one account, followed by the current user, that also follows targetUserId — for the "Followed by X" hint. Null when there is none or currentUserId is null.</summary>
        Task<string?> GetFollowedByUsernameAsync(Guid? currentUserId, Guid targetUserId);

        /// <summary>Ids of everyone userId actively follows — the audience for their home feed (paired with their own id by the caller).</summary>
        Task<List<Guid>> GetFollowingUserIdsAsync(Guid userId);

        /// <summary>
        /// "Suggested for you": people who follow userId back, then mutual connections
        /// (followed by someone userId follows), then newest other accounts as filler —
        /// self and everyone already followed are excluded throughout.
        /// </summary>
        Task<List<SuggestionCandidate>> GetSuggestionsAsync(Guid userId, int limit);
    }
}
