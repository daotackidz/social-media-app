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
    }
}
