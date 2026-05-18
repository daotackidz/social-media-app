using Microsoft.EntityFrameworkCore;
using Social.Data.Model.User;
using Social.Data.Repository;
using Social.Repository.Social.User.Interface;
using static Social.Data.Model.User.UserPendingRegistrations;

namespace Social.Repository.Social.User.Repository
{
    public class UserPendingRepository : IUserPendingRepository
    {
        private readonly SocialDbContext _db;

        public UserPendingRepository(SocialDbContext db) => _db = db;

        public async Task<UserPendingRegistrations?> GetLatestAsync(string email, PendingOtpType type)
            => await _db.UserPendingRegistrations
                .Where(p => p.Email == email
                         && p.Type == type
                         && !p.IsVerified)
                .OrderByDescending(p => p.CreatedDateUnix)
                .FirstOrDefaultAsync();

        public async Task AddAsync(UserPendingRegistrations pending)
        {
            pending.Id = Guid.NewGuid();
            pending.CreatedByUserId = pending.Id;
            await _db.UserPendingRegistrations.AddAsync(pending);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserPendingRegistrations pending)
        {
            _db.UserPendingRegistrations.Update(pending);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOldAsync(string email, PendingOtpType type)
        {
            var old = await _db.UserPendingRegistrations
                .Where(p => p.Email == email && p.Type == type && !p.IsVerified)
                .ToListAsync();

            if (old.Count != 0)
            {
                _db.UserPendingRegistrations.RemoveRange(old);
                await _db.SaveChangesAsync();
            }
        }
    }
}
