using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.Notification;
using Social.Data.Repository;
using Social.Repository.Social.Notification.Interface;

namespace Social.Repository.Social.Notification.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SocialDbContext _db;

        public NotificationRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task UpsertLikeNotificationAsync(Notifications notification)
        {
            var existing = await _db.Notifications.FirstOrDefaultAsync(n =>
                n.UserId == notification.UserId
                && n.ActorUserId == notification.ActorUserId
                && n.Type == notification.Type
                && n.PostId == notification.PostId
                && n.StoryId == notification.StoryId);

            if (existing is null)
            {
                await _db.Notifications.AddAsync(notification);
            }
            else
            {
                existing.RecordStatusId = RecordStatus.Status.Active;
                existing.IsRead = false;
                existing.CreatedDate = DateTime.UtcNow;
                existing.CreatedDateUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _db.Notifications.Update(existing);
            }

            await _db.SaveChangesAsync();
        }

        public async Task CreateAsync(Notifications notification)
        {
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Notifications>> GetPageAsync(Guid userId, int skip, int take)
            => await _db.Notifications
                .Where(n => n.UserId == userId && n.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(n => n.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead && n.RecordStatusId == RecordStatus.Status.Active);

        public async Task MarkAllReadAsync(Guid userId)
        {
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.RecordStatusId == RecordStatus.Status.Active)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public async Task<bool> MarkReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification is null) return false;

            notification.IsRead = true;
            _db.Notifications.Update(notification);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
