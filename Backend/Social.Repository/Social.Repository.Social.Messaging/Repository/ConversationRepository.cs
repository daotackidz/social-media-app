using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.Messaging;
using Social.Data.Repository;
using Social.Repository.Social.Messaging.Interface;

namespace Social.Repository.Social.Messaging.Repository
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly SocialDbContext _db;

        public ConversationRepository(SocialDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> GetOrCreateDirectConversationAsync(Guid userId1, Guid userId2)
        {
            // A direct conversation between these two is exactly the one whose active-participant
            // set is {userId1, userId2} — no more, no less.
            var candidateIds = await _db.ConversationParticipants
                .Where(p => (p.UserId == userId1 || p.UserId == userId2) && p.RecordStatusId == RecordStatus.Status.Active)
                .GroupBy(p => p.ConversationId)
                .Where(g => g.Count() == 2)
                .Select(g => g.Key)
                .ToListAsync();

            if (candidateIds.Count > 0)
            {
                var existingId = await _db.Conversations
                    .Where(c => candidateIds.Contains(c.Id) && !c.IsGroup && c.RecordStatusId == RecordStatus.Status.Active)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync();
                if (existingId.HasValue) return existingId.Value;
            }

            var conversation = new Conversations
            {
                Id = Guid.NewGuid(),
                IsGroup = false,
                CreatedByUserId = userId1
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Conversations.AddAsync(conversation);
                await _db.ConversationParticipants.AddRangeAsync(
                    new ConversationParticipants { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = userId1, CreatedByUserId = userId1 },
                    new ConversationParticipants { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = userId2, CreatedByUserId = userId1 }
                );
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return conversation.Id;
        }

        public async Task<List<ConversationSummary>> GetConversationsForUserAsync(Guid userId, int skip, int take)
        {
            var page = await (
                from p in _db.ConversationParticipants
                join c in _db.Conversations on p.ConversationId equals c.Id
                where p.UserId == userId && p.RecordStatusId == RecordStatus.Status.Active && c.RecordStatusId == RecordStatus.Status.Active
                orderby c.LastMessageAt descending, c.CreatedDate descending
                select new { p.ConversationId, p.LastReadAt })
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            if (page.Count == 0) return new List<ConversationSummary>();

            var conversationIds = page.Select(x => x.ConversationId).ToList();

            var others = await _db.ConversationParticipants
                .Where(p => conversationIds.Contains(p.ConversationId) && p.UserId != userId && p.RecordStatusId == RecordStatus.Status.Active)
                .Select(p => new { p.ConversationId, p.UserId, p.LastReadAt })
                .ToListAsync();
            var otherByConversation = others.GroupBy(o => o.ConversationId).ToDictionary(g => g.Key, g => g.First());

            var lastMessages = await _db.Messages
                .Where(m => conversationIds.Contains(m.ConversationId) && m.RecordStatusId == RecordStatus.Status.Active)
                .GroupBy(m => m.ConversationId)
                .Select(g => g.OrderByDescending(m => m.CreatedDate).First())
                .ToListAsync();
            var lastByConversation = lastMessages.ToDictionary(m => m.ConversationId);

            return page.Select(x =>
            {
                lastByConversation.TryGetValue(x.ConversationId, out var last);
                otherByConversation.TryGetValue(x.ConversationId, out var other);

                return new ConversationSummary
                {
                    ConversationId = x.ConversationId,
                    OtherUserId = other?.UserId ?? Guid.Empty,
                    LastMessageAt = last?.CreatedDate,
                    LastMessagePreview = last?.Content,
                    LastMessageType = last?.Type,
                    HasUnread = last is not null && last.SenderUserId != userId && (x.LastReadAt is null || last.CreatedDate > x.LastReadAt),
                    OtherLastReadAt = other?.LastReadAt
                };
            }).ToList();
        }

        public async Task<Conversations?> GetByIdAsync(Guid conversationId)
            => await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && c.RecordStatusId == RecordStatus.Status.Active);

        public async Task<bool> IsParticipantAsync(Guid conversationId, Guid userId)
            => await _db.ConversationParticipants.AnyAsync(p =>
                p.ConversationId == conversationId && p.UserId == userId && p.RecordStatusId == RecordStatus.Status.Active);

        public async Task<Guid?> GetOtherParticipantIdAsync(Guid conversationId, Guid excludingUserId)
            => await _db.ConversationParticipants
                .Where(p => p.ConversationId == conversationId && p.UserId != excludingUserId && p.RecordStatusId == RecordStatus.Status.Active)
                .Select(p => (Guid?)p.UserId)
                .FirstOrDefaultAsync();

        public async Task<DateTime?> GetLastReadAtAsync(Guid conversationId, Guid userId)
            => await _db.ConversationParticipants
                .Where(p => p.ConversationId == conversationId && p.UserId == userId && p.RecordStatusId == RecordStatus.Status.Active)
                .Select(p => p.LastReadAt)
                .FirstOrDefaultAsync();

        public async Task<List<Messages>> GetMessagesAsync(Guid conversationId, int skip, int take)
            => await _db.Messages
                .Include(m => m.Files)
                .Where(m => m.ConversationId == conversationId && m.RecordStatusId == RecordStatus.Status.Active)
                .OrderByDescending(m => m.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public async Task<Messages?> GetMessageByIdAsync(Guid messageId)
            => await _db.Messages
                .Include(m => m.Files)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.RecordStatusId == RecordStatus.Status.Active);

        public async Task<Dictionary<Guid, Messages>> GetMessagesByIdsAsync(IEnumerable<Guid> messageIds)
        {
            var ids = messageIds.ToList();
            if (ids.Count == 0) return new Dictionary<Guid, Messages>();

            var messages = await _db.Messages
                .Include(m => m.Files)
                .Where(m => ids.Contains(m.Id) && m.RecordStatusId == RecordStatus.Status.Active)
                .ToListAsync();

            return messages.ToDictionary(m => m.Id);
        }

        public async Task<Messages> AddMessageAsync(Guid conversationId, Guid senderId, string? content, Messages.MessageType type, Guid? fileId, Guid? replyToMessageId)
        {
            var message = new Messages
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderUserId = senderId,
                Content = content,
                Type = type,
                FileId = fileId,
                ReplyToMessageId = replyToMessageId,
                CreatedByUserId = senderId
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Messages.AddAsync(message);

                var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
                if (conversation is not null)
                {
                    conversation.LastMessageAt = DateTime.UtcNow;
                    _db.Conversations.Update(conversation);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            if (fileId.HasValue) await _db.Entry(message).Reference(m => m.Files).LoadAsync();
            return message;
        }

        public async Task MarkReadAsync(Guid conversationId, Guid userId)
        {
            var participant = await _db.ConversationParticipants.FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant is null) return;

            participant.LastReadAt = DateTime.UtcNow;
            _db.ConversationParticipants.Update(participant);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool Removed, string? Emoji)> ToggleReactionAsync(Guid messageId, Guid userId, string emoji)
        {
            var existing = await _db.MessageReactions.FirstOrDefaultAsync(r =>
                r.MessageId == messageId && r.UserId == userId && r.RecordStatusId == RecordStatus.Status.Active);

            if (existing is null)
            {
                await _db.MessageReactions.AddAsync(new MessageReactions
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji,
                    CreatedByUserId = userId
                });
                await _db.SaveChangesAsync();
                return (false, emoji);
            }

            if (existing.Emoji == emoji)
            {
                existing.RecordStatusId = RecordStatus.Status.Deleted;
                _db.MessageReactions.Update(existing);
                await _db.SaveChangesAsync();
                return (true, null);
            }

            existing.Emoji = emoji;
            _db.MessageReactions.Update(existing);
            await _db.SaveChangesAsync();
            return (false, emoji);
        }

        public async Task<Dictionary<Guid, List<MessageReactionInfo>>> GetReactionsAsync(IEnumerable<Guid> messageIds)
        {
            var ids = messageIds.ToList();
            if (ids.Count == 0) return new Dictionary<Guid, List<MessageReactionInfo>>();

            var rows = await _db.MessageReactions
                .Where(r => ids.Contains(r.MessageId) && r.RecordStatusId == RecordStatus.Status.Active)
                .Select(r => new { r.MessageId, r.UserId, r.Emoji })
                .ToListAsync();

            return rows
                .GroupBy(r => r.MessageId)
                .ToDictionary(g => g.Key, g => g.Select(r => new MessageReactionInfo { UserId = r.UserId, Emoji = r.Emoji }).ToList());
        }
    }
}
