using Social.Data.Model.Messaging;

namespace Social.Repository.Social.Messaging.Interface
{
    /// <summary>One row of the conversation list — the repository only knows the raw thread + participant data, not display info (that's IUserRepository's job).</summary>
    public class ConversationSummary
    {
        public Guid ConversationId { get; set; }

        /// <summary>The other participant in a 1-1 conversation.</summary>
        public Guid OtherUserId { get; set; }

        public DateTime? LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public Messages.MessageType? LastMessageType { get; set; }
        public bool HasUnread { get; set; }

        /// <summary>When the other participant last read this conversation — lets the caller tell whether their own latest message has been seen.</summary>
        public DateTime? OtherLastReadAt { get; set; }
    }

    /// <summary>One user's reaction on one message.</summary>
    public class MessageReactionInfo
    {
        public Guid UserId { get; set; }
        public string Emoji { get; set; } = string.Empty;
    }

    public interface IConversationRepository
    {
        /// <summary>Finds the existing 1-1 conversation between exactly these two users, or creates one.</summary>
        Task<Guid> GetOrCreateDirectConversationAsync(Guid userId1, Guid userId2);

        /// <summary>userId's conversations, most recently active first. Fetches take+1 so the caller can tell whether another page exists without a separate count query.</summary>
        Task<List<ConversationSummary>> GetConversationsForUserAsync(Guid userId, int skip, int take);

        Task<Conversations?> GetByIdAsync(Guid conversationId);

        Task<bool> IsParticipantAsync(Guid conversationId, Guid userId);

        /// <summary>The other side of a 1-1 conversation, or null if userId is the only active participant left.</summary>
        Task<Guid?> GetOtherParticipantIdAsync(Guid conversationId, Guid excludingUserId);

        /// <summary>When userId last read conversationId, or null if they never have (or aren't a participant).</summary>
        Task<DateTime?> GetLastReadAtAsync(Guid conversationId, Guid userId);

        /// <summary>Newest first (the natural order to page "load older on scroll up" from) — the caller reverses for display.</summary>
        Task<List<Messages>> GetMessagesAsync(Guid conversationId, int skip, int take);

        /// <summary>One message by id (with its file loaded), or null if it doesn't exist — used to validate/hydrate a reply's quoted message.</summary>
        Task<Messages?> GetMessageByIdAsync(Guid messageId);

        /// <summary>Messages by id (with files loaded), keyed by id — batch version of GetMessageByIdAsync for hydrating a page of replies' quoted previews in one query.</summary>
        Task<Dictionary<Guid, Messages>> GetMessagesByIdsAsync(IEnumerable<Guid> messageIds);

        /// <summary>
        /// Inserts the message and bumps the conversation's denormalized LastMessageAt in one
        /// transaction. replyToMessageId, when given, must already be a message in the same
        /// conversation — the caller (MessagesController) is responsible for that check.
        /// </summary>
        Task<Messages> AddMessageAsync(Guid conversationId, Guid senderId, string? content, Messages.MessageType type, Guid? fileId, Guid? replyToMessageId);

        Task MarkReadAsync(Guid conversationId, Guid userId);

        /// <summary>
        /// Sets userId's reaction on messageId to emoji — inserts it if they had none, swaps it if they'd
        /// picked a different emoji, or removes it if they pick the same one again. Returns the reaction's
        /// new state (Removed=true means their reaction is now gone; otherwise Emoji is the current value).
        /// </summary>
        Task<(bool Removed, string? Emoji)> ToggleReactionAsync(Guid messageId, Guid userId, string emoji);

        /// <summary>Active reactions for each of messageIds, keyed by message id — for hydrating a page of messages in one query.</summary>
        Task<Dictionary<Guid, List<MessageReactionInfo>>> GetReactionsAsync(IEnumerable<Guid> messageIds);
    }
}
