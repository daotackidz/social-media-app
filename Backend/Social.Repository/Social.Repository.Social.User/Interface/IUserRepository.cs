using Social.Data.Model.File;
using Social.Data.Model.User;

namespace Social.Repository.Social.User.Interface
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task CreateUserAsync(Users user, UserProfiles profile);
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByUserNameAsync(string username);
        Task<UserProfiles?> GetProfileByUserIdAsync(Guid userId);
        Task<List<UserHighlights>> GetHighlightsByUserIdAsync(Guid userId);
        Task AddProfileAsync(UserProfiles profile);
        Task UpdateProfileAsync(UserProfiles profile);
        Task<UserFiles?> GetPrimaryAvatarAsync(Guid userId);
        Task UnsetPrimaryAvatarAsync(Guid userId);
        Task AddFileAsync(Files file);
        Task AddUserFileAsync(UserFiles userFile);
        Task UpdateAsync(Users user);

        /// <summary>Active users whose username/full name matches the query, for the search popup. Excludes excludeUserId (the current user) when given.</summary>
        Task<List<Users>> SearchUsersAsync(string query, Guid? excludeUserId, int take);

        /// <summary>Batch profile lookup keyed by UserId — avoids one round-trip per user when rendering a list.</summary>
        Task<Dictionary<Guid, UserProfiles>> GetProfilesByUserIdsAsync(IEnumerable<Guid> userIds);

        /// <summary>Batch user lookup keyed by Id — e.g. resolving usernames for a page of feed posts/stories.</summary>
        Task<Dictionary<Guid, Users>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);

        /// <summary>Batch primary-avatar URL lookup keyed by UserId.</summary>
        Task<Dictionary<Guid, string>> GetPrimaryAvatarUrlsByUserIdsAsync(IEnumerable<Guid> userIds);
    }
}
