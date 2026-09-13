using Social.Data.Model.Post;

namespace Social.Repository.Social.Post.Interface
{
    public interface IPostRepository
    {
        Task CreateAsync(Posts post, List<PostFiles> postFiles);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<List<Posts>> GetByUserIdAsync(Guid userId, int skip, int take);
        Task<List<Posts>> GetTaggedByUserIdAsync(Guid userId, int skip, int take);

        /// <summary>
        /// Home feed page: posts by ownerUserIds (self + followed, decided by the caller),
        /// newest first. Fetches take+1 so the caller can tell whether another page exists
        /// without a separate count query.
        /// </summary>
        Task<List<Posts>> GetFeedPageAsync(IEnumerable<Guid> ownerUserIds, int skip, int take);

        /// <summary>One post with its files loaded — backs the "open post" detail overlay.</summary>
        Task<Posts?> GetByIdAsync(Guid postId);

        /// <summary>Top-level comments on a post (replies excluded), oldest first (reading order), paged.</summary>
        Task<List<PostComments>> GetTopLevelCommentsAsync(Guid postId, int skip, int take);

        /// <summary>Replies under one top-level comment, oldest first, paged.</summary>
        Task<List<PostComments>> GetRepliesAsync(Guid parentCommentId, int skip, int take);

        /// <summary>Of parentCommentIds, how many active replies each has — for the "Xem N câu trả lời" link on each top-level comment.</summary>
        Task<Dictionary<Guid, int>> GetReplyCountsAsync(IEnumerable<Guid> parentCommentIds);

        /// <summary>One comment (or reply) by id, or null if it doesn't exist / was deleted.</summary>
        Task<PostComments?> GetCommentByIdAsync(Guid commentId);

        /// <summary>
        /// Inserts the comment and bumps the post's denormalized CommentCount in one transaction.
        /// parentId, when given, must already be the flattened (top-level) parent — callers resolve
        /// "reply to a reply" up to its own top-level comment before calling this.
        /// </summary>
        Task<PostComments> AddCommentAsync(Guid postId, Guid userId, string content, Guid? parentId);

        /// <summary>
        /// Likes commentId for userId if not already liked, unlikes it if it is (same soft-delete/
        /// reactivate pattern as post likes). Bumps the comment's denormalized LikeCount in the same
        /// transaction. Returns null if the comment doesn't exist.
        /// </summary>
        Task<(bool Liked, int LikeCount)?> ToggleCommentLikeAsync(Guid commentId, Guid userId);

        /// <summary>Of commentIds, which ones userId currently has an active like on — for hydrating a comment page's IsLiked flags in one query.</summary>
        Task<HashSet<Guid>> GetLikedCommentIdsAsync(IEnumerable<Guid> commentIds, Guid userId);

        /// <summary>User ids who actively like commentId, most recent like first — backs the comment's own "Lượt thích" popup.</summary>
        Task<List<Guid>> GetCommentLikerUserIdsAsync(Guid commentId, int skip, int take);

        /// <summary>
        /// Likes postId for userId if not already liked, unlikes it if it is (soft-delete/
        /// reactivate, same pattern as follow/unfollow — a like row is never hard-deleted).
        /// Bumps the post's denormalized LikeCount in the same transaction. Returns null if
        /// the post doesn't exist.
        /// </summary>
        Task<(bool Liked, int LikeCount)?> ToggleLikeAsync(Guid postId, Guid userId);

        /// <summary>Whether userId currently has an active like on postId.</summary>
        Task<bool> IsLikedByUserAsync(Guid postId, Guid userId);

        /// <summary>Of postIds, which ones userId currently has an active like on — for hydrating a feed page's IsLiked flags in one query.</summary>
        Task<HashSet<Guid>> GetLikedPostIdsAsync(IEnumerable<Guid> postIds, Guid userId);

        /// <summary>Total number of active likes on a post — for paging the "likes" list.</summary>
        Task<int> GetLikeCountAsync(Guid postId);

        /// <summary>User ids who actively like postId, most recent like first — backs the "Lượt thích" popup.</summary>
        Task<List<Guid>> GetLikerUserIdsAsync(Guid postId, int skip, int take);
    }
}
