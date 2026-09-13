using Social.Data.Model.Messaging;

namespace Social.Data.Model.Response.Messaging
{
    /// <summary>One row of the conversation list, or the header info for one open thread — this app only has 1-1 conversations today, so "the other participant" is always well-defined.</summary>
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public Guid OtherUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? LastMessagePreview { get; set; }
        public Messages.MessageType? LastMessageType { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool HasUnread { get; set; }

        /// <summary>When the other participant last read this conversation — lets the client show "Đã xem" under the caller's own latest message once it's on or before this.</summary>
        public DateTime? OtherLastReadAt { get; set; }
    }

    /// <summary>One reaction on a message — this app only has 1-1 conversations, so at most two of these exist per message.</summary>
    public class MessageReactionResponse
    {
        public Guid UserId { get; set; }
        public string Emoji { get; set; } = string.Empty;
    }

    /// <summary>Compact snapshot of the quoted message on a reply — enough to render the quote box without a second round-trip.</summary>
    public class MessageReplyPreviewResponse
    {
        public Guid Id { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? Content { get; set; }
        public Messages.MessageType Type { get; set; }
        public string? FileUrl { get; set; }
    }

    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? Content { get; set; }
        public Messages.MessageType Type { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<MessageReactionResponse> Reactions { get; set; } = new();

        /// <summary>Set when this message replies to another one (the "Trả lời" feature) — null otherwise.</summary>
        public MessageReplyPreviewResponse? ReplyTo { get; set; }
    }
}
