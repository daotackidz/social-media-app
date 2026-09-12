using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.File;
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

        public async Task<bool> UsernameExistsAsync(string username)
            => await _db.Users.AnyAsync(u => EF.Functions.ILike(u.UserName, username));

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

        public async Task<Users?> GetByUserNameAsync(string username)
            => await _db.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName, username));

        public async Task<UserProfiles?> GetProfileByUserIdAsync(Guid userId)
            => await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        public async Task<List<UserHighlights>> GetHighlightsByUserIdAsync(Guid userId)
            => await _db.UserHighlights
                .Include(x => x.CoverFile)
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.CreatedDate)
                .ToListAsync();

        public async Task AddProfileAsync(UserProfiles profile)
        {
            await _db.UserProfiles.AddAsync(profile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(UserProfiles profile)
        {
            _db.UserProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }

        public async Task<UserFiles?> GetPrimaryAvatarAsync(Guid userId)
            => await _db.UserFiles
                .Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.UserId == userId
                                        && x.FileType == UserFiles.UserFileType.Avatar
                                        && x.IsPrimary);

        public async Task UnsetPrimaryAvatarAsync(Guid userId)
        {
            var currentAvatars = await _db.UserFiles
                .Where(x => x.UserId == userId
                         && x.FileType == UserFiles.UserFileType.Avatar
                         && x.IsPrimary)
                .ToListAsync();

            if (!currentAvatars.Any())
                return;

            foreach (var avatar in currentAvatars)
                avatar.IsPrimary = false;

            _db.UserFiles.UpdateRange(currentAvatars);
            await _db.SaveChangesAsync();
        }

        public async Task AddFileAsync(Files file)
        {
            await _db.Files.AddAsync(file);
            await _db.SaveChangesAsync();
        }

        public async Task AddUserFileAsync(UserFiles userFile)
        {
            await _db.UserFiles.AddAsync(userFile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Users user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Users>> SearchUsersAsync(string query, Guid? excludeUserId, int take)
        {
            var q = query.Trim();
            if (q.Length == 0) return new List<Users>();

            return await _db.Users
                .Where(u => u.RecordStatusId == RecordStatus.Status.Active
                         && (!excludeUserId.HasValue || u.Id != excludeUserId.Value)
                         && (EF.Functions.ILike(u.UserName, $"%{q}%")
                             || _db.UserProfiles.Any(p => p.UserId == u.Id && EF.Functions.ILike(p.FullName, $"%{q}%"))))
                .OrderBy(u => u.UserName)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, UserProfiles>> GetProfilesByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, UserProfiles>();

            return await _db.UserProfiles
                .Where(p => ids.Contains(p.UserId))
                .GroupBy(p => p.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.First());
        }

        public async Task<Dictionary<Guid, Users>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, Users>();

            return await _db.Users
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);
        }

        public async Task<Dictionary<Guid, string>> GetPrimaryAvatarUrlsByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, string>();

            return await _db.UserFiles
                .Include(x => x.Files)
                .Where(x => ids.Contains(x.UserId)
                         && x.FileType == UserFiles.UserFileType.Avatar
                         && x.IsPrimary
                         && x.Files != null)
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.First().Files!.StoragePath);
        }
    }
}
