using Social.Data.Model.User;
using static Social.Data.Model.User.UserPendingRegistrations;

namespace Social.Repository.Social.User.Interface
{
    public interface IUserPendingRepository
    {
        Task<UserPendingRegistrations?> GetLatestAsync(string email, PendingOtpType type);
        Task AddAsync(UserPendingRegistrations pending);
        Task UpdateAsync(UserPendingRegistrations pending);
        Task DeleteOldAsync(string email, PendingOtpType type);
    }
}
