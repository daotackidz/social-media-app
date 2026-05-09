using Microsoft.EntityFrameworkCore;
using Social.Data.Model.User;
using Social.Data.Repository;
using Social.Repository.Social.User.Interface;

namespace Social.Repository.Social.User.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SocialDbContext _db;

        public UserRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);

        public async Task CreateUserAsync(Users user, UserProfiles profile)
        {
            // Dùng transaction để đảm bảo insert cả 2 hoặc không cái nào
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                profile.UserId = user.Id; // Gán FK sau khi có UserId
                await _db.UserProfiles.AddAsync(profile);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Users?> GetByEmailAsync(string email)
            => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task UpdateAsync(Users user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
