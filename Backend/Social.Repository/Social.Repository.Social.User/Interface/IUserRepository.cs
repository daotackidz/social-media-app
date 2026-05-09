using Social.Data.Model.User;

namespace Social.Repository.Social.User.Interface
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task CreateUserAsync(Users user, UserProfiles profile);
        Task<Users?> GetByEmailAsync(string email);
        Task UpdateAsync(Users user);
    }
}
