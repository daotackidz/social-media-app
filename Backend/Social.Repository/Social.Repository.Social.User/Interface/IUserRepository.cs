using Social.Data.Model.File;
using Social.Data.Model.User;

namespace Social.Repository.Social.User.Interface
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task CreateUserAsync(Users user, UserProfiles profile);
        Task<Users?> GetByEmailAsync(string email);
        Task<UserProfiles?> GetProfileByUserIdAsync(Guid userId);
        Task AddProfileAsync(UserProfiles profile);
        Task UpdateProfileAsync(UserProfiles profile);
        Task<UserFiles?> GetPrimaryAvatarAsync(Guid userId);
        Task UnsetPrimaryAvatarAsync(Guid userId);
        Task AddFileAsync(Files file);
        Task AddUserFileAsync(UserFiles userFile);
        Task UpdateAsync(Users user);
    }
}
