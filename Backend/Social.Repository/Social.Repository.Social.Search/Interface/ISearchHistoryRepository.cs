using Social.Data.Model.User;

namespace Social.Repository.Social.Search.Interface
{
    public interface ISearchHistoryRepository
    {
        Task<List<UserSearchHistories>> GetRecentAsync(Guid userId, int take);
        Task AddOrTouchAsync(Guid userId, Guid targetUserId);
        Task<bool> SoftDeleteAsync(Guid userId, Guid id);
        Task SoftDeleteAllAsync(Guid userId);
    }
}
