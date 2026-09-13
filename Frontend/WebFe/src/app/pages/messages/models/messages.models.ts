export type MessageType = 'Text' | 'Image' | 'Video' | 'Audio';

/** One row of the conversation list, or a thread's header info — mirrors ConversationResponse. */
export interface ConversationSummary {
  id: string;
  otherUserId: string;
  username: string;
  avatarUrl?: string;
  lastMessagePreview?: string;
  lastMessageType?: MessageType;
  lastMessageAt?: string;
  hasUnread: boolean;
  /** When the other participant last read this conversation — used to show "Đã xem" under the caller's own latest message. */
  otherLastReadAt?: string;
}

export interface MessageReaction {
  userId: string;
  emoji: string;
}

/** Compact snapshot of the quoted message on a reply — mirrors MessageReplyPreviewResponse. */
export interface MessageReplyPreview {
  id: string;
  senderUserId: string;
  senderUsername: string;
  content?: string;
  type: MessageType;
  fileUrl?: string;
}

export interface ChatMessage {
  id: string;
  senderUserId: string;
  senderUsername: string;
  content?: string;
  type: MessageType;
  fileUrl?: string;
  createdDate: string;
  reactions: MessageReaction[];
  /** Set when this message replies to another one (the "Trả lời" feature) — undefined otherwise. */
  replyTo?: MessageReplyPreview;
}
