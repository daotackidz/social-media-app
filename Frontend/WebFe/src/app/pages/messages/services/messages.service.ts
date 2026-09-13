import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../core/api';
import { ChatMessage, ConversationSummary, MessageReaction, MessageReplyPreview, MessageType } from '../models/messages.models';

interface ConversationDto {
  id: string;
  otherUserId: string;
  username: string;
  avatarUrl?: string | null;
  lastMessagePreview?: string | null;
  lastMessageType?: MessageType | null;
  lastMessageAt?: string | null;
  hasUnread: boolean;
  otherLastReadAt?: string | null;
}

interface MessageReactionDto {
  userId: string;
  emoji: string;
}

interface MessageReplyPreviewDto {
  id: string;
  senderUserId: string;
  senderUsername: string;
  content?: string | null;
  type: MessageType;
  fileUrl?: string | null;
}

interface MessageDto {
  id: string;
  senderUserId: string;
  senderUsername: string;
  content?: string | null;
  type: MessageType;
  fileUrl?: string | null;
  createdDate: string;
  reactions: MessageReactionDto[];
  replyTo?: MessageReplyPreviewDto | null;
}

function toConversation(dto: ConversationDto): ConversationSummary {
  return {
    id: dto.id,
    otherUserId: dto.otherUserId,
    username: dto.username,
    avatarUrl: dto.avatarUrl ?? undefined,
    lastMessagePreview: dto.lastMessagePreview ?? undefined,
    lastMessageType: dto.lastMessageType ?? undefined,
    lastMessageAt: dto.lastMessageAt ?? undefined,
    hasUnread: dto.hasUnread,
    otherLastReadAt: dto.otherLastReadAt ?? undefined
  };
}

function toReaction(dto: MessageReactionDto): MessageReaction {
  return { userId: dto.userId, emoji: dto.emoji };
}

function toReplyPreview(dto: MessageReplyPreviewDto): MessageReplyPreview {
  return {
    id: dto.id,
    senderUserId: dto.senderUserId,
    senderUsername: dto.senderUsername,
    content: dto.content ?? undefined,
    type: dto.type,
    fileUrl: dto.fileUrl ?? undefined
  };
}

function toMessage(dto: MessageDto): ChatMessage {
  return {
    id: dto.id,
    senderUserId: dto.senderUserId,
    senderUsername: dto.senderUsername,
    content: dto.content ?? undefined,
    type: dto.type,
    fileUrl: dto.fileUrl ?? undefined,
    createdDate: dto.createdDate,
    reactions: (dto.reactions ?? []).map(toReaction),
    replyTo: dto.replyTo ? toReplyPreview(dto.replyTo) : undefined
  };
}

/** Direct messages — mirrors Backend/Social.WebApi/Controllers/MessagesController.cs. No realtime transport: the open thread polls getMessages(). */
@Injectable({ providedIn: 'root' })
export class MessagesService {
  private readonly http = inject(HttpClient);

  getConversations(skip = 0, take = 30): Observable<{ items: ConversationSummary[]; hasMore: boolean }> {
    return this.http.get<ApiResponse<{ items: ConversationDto[]; hasMore: boolean }>>('/api/messages/conversations', { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return { items: data.items.map(toConversation), hasMore: data.hasMore };
      })
    );
  }

  /** Gets or creates the 1-1 thread with this username — the "Tin nhắn mới" picker's "Chat" action. */
  startConversation(username: string): Observable<ConversationSummary> {
    return this.http.post<ApiResponse<ConversationDto>>('/api/messages/conversations', { username }).pipe(map((res) => toConversation(res.data!)));
  }

  getConversation(id: string): Observable<ConversationSummary> {
    return this.http.get<ApiResponse<ConversationDto>>(`/api/messages/conversations/${id}`).pipe(map((res) => toConversation(res.data!)));
  }

  /** Newest first — the caller reverses for display. */
  getMessages(conversationId: string, skip = 0, take = 30): Observable<{ items: ChatMessage[]; hasMore: boolean }> {
    return this.http
      .get<ApiResponse<{ items: MessageDto[]; hasMore: boolean }>>(`/api/messages/conversations/${conversationId}/messages`, { params: { skip, take } })
      .pipe(
        map((res) => {
          const data = res.data ?? { items: [], hasMore: false };
          return { items: data.items.map(toMessage), hasMore: data.hasMore };
        })
      );
  }

  /** file, when given, decides the message's type (image/video/audio) from its content type; content is an optional caption/text body. replyToMessageId quotes another message in this conversation (the "Trả lời" feature). */
  sendMessage(conversationId: string, content: string, file?: File | Blob, fileName?: string, replyToMessageId?: string): Observable<ChatMessage> {
    const formData = new FormData();
    if (content.trim()) formData.append('Content', content.trim());
    if (file) formData.append('File', file, fileName ?? (file instanceof File ? file.name : 'attachment'));
    if (replyToMessageId) formData.append('ReplyToMessageId', replyToMessageId);

    return this.http
      .post<ApiResponse<MessageDto>>(`/api/messages/conversations/${conversationId}/messages`, formData)
      .pipe(map((res) => toMessage(res.data!)));
  }

  markRead(conversationId: string): Observable<void> {
    return this.http.post<ApiResponse<null>>(`/api/messages/conversations/${conversationId}/read`, {}).pipe(map(() => undefined));
  }

  /** Sets the caller's reaction on messageId — a new emoji, swapped for a different one, or cleared if they pick the same one again. Returns that message's full reaction list. */
  reactToMessage(conversationId: string, messageId: string, emoji: string): Observable<MessageReaction[]> {
    return this.http
      .post<ApiResponse<MessageReactionDto[]>>(`/api/messages/conversations/${conversationId}/messages/${messageId}/react`, { emoji })
      .pipe(map((res) => (res.data ?? []).map(toReaction)));
  }
}
