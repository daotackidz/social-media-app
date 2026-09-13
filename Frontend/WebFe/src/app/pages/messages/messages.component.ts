import { Component, ElementRef, HostListener, OnDestroy, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { LanguageService } from '../../core/i18n/language.service';
import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';
import { ChatMessage, ConversationSummary, MessageReaction } from './models/messages.models';
import { MessagesService } from './services/messages.service';

/** How often the open thread re-fetches its latest page while visible — there's no realtime transport. */
const POLL_MS = 3000;

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [RouterLink, SidebarNavComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly messagesService = inject(MessagesService);
  private readonly dialog = inject(MatDialog);
  private readonly languageService = inject(LanguageService);

  readonly sidebarUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });
  readonly sidebarAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  readonly conversations = signal<ConversationSummary[]>([]);
  readonly conversationsLoading = signal(true);
  readonly listFilter = signal('');

  readonly activeConversationId = signal<string | null>(null);
  readonly activeConversation = signal<ConversationSummary | null>(null);
  readonly messages = signal<ChatMessage[]>([]);
  readonly threadLoading = signal(false);
  readonly threadLoadError = signal(false);

  readonly draft = signal('');
  readonly sending = signal(false);
  readonly recording = signal(false);
  readonly recordingUnsupported = signal(false);

  readonly emojiPickerOpen = signal(false);
  readonly commonEmojis = [
    '😂', '😮', '😞', '😡', '👏', '🔥', '🎉', '💯',
    '❤️', '🤣', '🤗', '🤔', '☺️', '😊',
    '😍', '😘', '😭', '😅', '😁', '😉', '🙌', '🙏',
    '👍', '👎', '👋', '💪', '✨', '🎂', '😎', '🥳'
  ];

  /** Which message's reaction picker is open, if any. */
  readonly reactionPickerMessageId = signal<string | null>(null);
  readonly reactionEmojis = ['❤️', '😆', '😮', '😢', '😡', '👍'];

  /** Which message's "who reacted" list is open, if any. */
  readonly reactionsListMessage = signal<ChatMessage | null>(null);

  /** The message being replied to, if any — shown as a "Đang trả lời [username]" bar above the composer. */
  readonly replyTarget = signal<ChatMessage | null>(null);

  private readonly messagesContainer = viewChild<ElementRef<HTMLElement>>('messagesContainer');

  private pollHandle?: ReturnType<typeof setInterval>;
  private mediaRecorder?: MediaRecorder;
  private recordedChunks: Blob[] = [];

  get filteredConversations(): ConversationSummary[] {
    const filter = this.listFilter().trim().toLowerCase();
    if (!filter) return this.conversations();
    return this.conversations().filter((c) => c.username.toLowerCase().includes(filter));
  }

  ngOnInit(): void {
    this.loadConversations();
    this.route.paramMap.subscribe((params) => this.openConversation(params.get('id')));
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  onFilterInput(event: Event): void {
    this.listFilter.set((event.target as HTMLInputElement).value);
  }

  private loadConversations(): void {
    this.messagesService.getConversations().subscribe({
      next: ({ items }) => {
        this.conversations.set(items);
        this.conversationsLoading.set(false);
      },
      error: () => this.conversationsLoading.set(false)
    });
  }

  private openConversation(id: string | null): void {
    this.stopPolling();
    this.activeConversationId.set(id);
    this.activeConversation.set(null);
    this.messages.set([]);
    this.threadLoadError.set(false);
    this.draft.set('');
    this.replyTarget.set(null);
    if (!id) return;

    this.threadLoading.set(true);
    this.messagesService.getConversation(id).subscribe({
      next: (c) => this.activeConversation.set(c),
      error: () => this.threadLoadError.set(true)
    });

    this.messagesService.getMessages(id, 0, 30).subscribe({
      next: ({ items }) => {
        this.messages.set([...items].reverse());
        this.threadLoading.set(false);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => {
        this.threadLoadError.set(true);
        this.threadLoading.set(false);
      }
    });

    this.messagesService.markRead(id).subscribe();
    this.markConversationRead(id);
    this.pollHandle = setInterval(() => this.pollNewMessages(id), POLL_MS);
  }

  private stopPolling(): void {
    if (this.pollHandle) clearInterval(this.pollHandle);
    this.pollHandle = undefined;
  }

  private pollNewMessages(id: string): void {
    this.messagesService.getMessages(id, 0, 30).subscribe({
      next: ({ items }) => {
        const newest = [...items].reverse();
        const knownIds = new Set(this.messages().map((m) => m.id));
        const added = newest.filter((m) => !knownIds.has(m.id));
        if (added.length > 0) {
          this.messages.update((list) => [...list, ...added]);
          queueMicrotask(() => this.scrollToBottom());
          this.messagesService.markRead(id).subscribe();
        }
      },
      error: () => undefined
    });

    // Refreshes otherLastReadAt too, so "Đã xem" appears under the caller's own latest
    // message as soon as the other side actually reads it, without needing a realtime push.
    this.messagesService.getConversation(id).subscribe({
      next: (c) => this.activeConversation.set(c),
      error: () => undefined
    });
  }

  openConversationRoute(id: string): void {
    this.router.navigate(['/messages', id]);
  }

  async openNewMessage(): Promise<void> {
    const { NewMessageModalComponent } = await import('./components/new-message-modal/new-message-modal.component');
    const ref = this.dialog.open<import('./components/new-message-modal/new-message-modal.component').NewMessageModalComponent, unknown, string | undefined>(
      NewMessageModalComponent,
      { autoFocus: false, panelClass: 'create-post-dialog-panel' }
    );

    ref.afterClosed().subscribe((conversationId) => {
      if (!conversationId) return;
      this.loadConversations();
      this.router.navigate(['/messages', conversationId]);
    });
  }

  onDraftInput(event: Event): void {
    this.draft.set((event.target as HTMLTextAreaElement).value);
  }

  onDraftKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  send(): void {
    const id = this.activeConversationId();
    const content = this.draft().trim();
    if (!id || !content || this.sending()) return;

    const replyToMessageId = this.replyTarget()?.id;
    this.sending.set(true);
    this.messagesService.sendMessage(id, content, undefined, undefined, replyToMessageId).subscribe({
      next: (message) => {
        this.messages.update((list) => [...list, message]);
        this.draft.set('');
        this.sending.set(false);
        this.replyTarget.set(null);
        this.bumpConversationPreview(id, message);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => this.sending.set(false)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0);
    input.value = '';
    if (file) this.sendAttachment(file);
  }

  private sendAttachment(file: File | Blob, fileName?: string): void {
    const id = this.activeConversationId();
    if (!id || this.sending()) return;

    const replyToMessageId = this.replyTarget()?.id;
    this.sending.set(true);
    this.messagesService.sendMessage(id, '', file, fileName, replyToMessageId).subscribe({
      next: (message) => {
        this.messages.update((list) => [...list, message]);
        this.sending.set(false);
        this.replyTarget.set(null);
        this.bumpConversationPreview(id, message);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => this.sending.set(false)
    });
  }

  /** Click starts recording; clicking again (or the composer's own stop icon) stops it and immediately sends the clip as a voice message. */
  async toggleRecording(): Promise<void> {
    if (this.recording()) {
      this.mediaRecorder?.stop();
      return;
    }

    if (!navigator.mediaDevices?.getUserMedia || typeof MediaRecorder === 'undefined') {
      this.recordingUnsupported.set(true);
      setTimeout(() => this.recordingUnsupported.set(false), 3000);
      return;
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.recordedChunks = [];
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) this.recordedChunks.push(e.data);
      };
      this.mediaRecorder.onstop = () => {
        stream.getTracks().forEach((t) => t.stop());
        this.recording.set(false);
        const blob = new Blob(this.recordedChunks, { type: this.mediaRecorder?.mimeType || 'audio/webm' });
        if (blob.size > 0) this.sendAttachment(blob, 'voice-message.webm');
      };
      this.mediaRecorder.start();
      this.recording.set(true);
    } catch {
      // Mic permission denied/unavailable — a silent no-op, same as this app's other best-effort browser features.
    }
  }

  private bumpConversationPreview(conversationId: string, message: ChatMessage): void {
    this.conversations.update((list) => {
      const index = list.findIndex((c) => c.id === conversationId);
      if (index === -1) return list;
      const updated: ConversationSummary = {
        ...list[index],
        lastMessagePreview: message.content,
        lastMessageType: message.type,
        lastMessageAt: message.createdDate,
        hasUnread: false
      };
      return [updated, ...list.slice(0, index), ...list.slice(index + 1)];
    });
  }

  private markConversationRead(conversationId: string): void {
    this.conversations.update((list) => list.map((c) => (c.id === conversationId ? { ...c, hasUnread: false } : c)));
  }

  isMine(message: ChatMessage): boolean {
    return message.senderUsername === this.authService.currentUsername();
  }

  /**
   * "Đã xem" shows only under the very last message, and only while it's still the last thing
   * said — once the other side replies, a new message follows it and the label no longer applies.
   */
  isSeenIndicator(message: ChatMessage): boolean {
    const list = this.messages();
    if (list.length === 0 || list[list.length - 1].id !== message.id) return false;
    if (!this.isMine(message)) return false;

    const readAt = this.activeConversation()?.otherLastReadAt;
    if (!readAt) return false;

    return new Date(readAt).getTime() >= new Date(message.createdDate).getTime();
  }

  isReactionPickerOpen(messageId: string): boolean {
    return this.reactionPickerMessageId() === messageId;
  }

  toggleReactionPicker(messageId: string, event: Event): void {
    event.stopPropagation();
    this.reactionPickerMessageId.update((current) => (current === messageId ? null : messageId));
  }

  /** Sets/replaces the caller's reaction, or clears it if they pick the same emoji they'd already chosen. */
  pickReaction(message: ChatMessage, emoji: string): void {
    const id = this.activeConversationId();
    if (!id) return;
    this.reactionPickerMessageId.set(null);

    this.messagesService.reactToMessage(id, message.id, emoji).subscribe({
      next: (reactions) => {
        this.messages.update((list) => list.map((m) => (m.id === message.id ? { ...m, reactions } : m)));
      },
      error: () => undefined
    });
  }

  /** Distinguishes the other participant's reaction from the caller's own — this app only has 1-1 threads. */
  isOtherReaction(reaction: MessageReaction): boolean {
    return reaction.userId === this.activeConversation()?.otherUserId;
  }

  /** The caller's own current reaction on this message, if any — highlighted in the picker so they can see what's already selected. */
  myReactionEmoji(message: ChatMessage): string | null {
    const otherUserId = this.activeConversation()?.otherUserId;
    return message.reactions.find((r) => r.userId !== otherUserId)?.emoji ?? null;
  }

  openReactionsList(message: ChatMessage, event: Event): void {
    event.stopPropagation();
    this.reactionsListMessage.set(message);
  }

  closeReactionsList(): void {
    this.reactionsListMessage.set(null);
  }

  /** Tapping your own row in the "who reacted" list removes it — same toggle as picking the same emoji again. */
  removeMyReaction(message: ChatMessage): void {
    const emoji = this.myReactionEmoji(message);
    if (!emoji) return;
    this.pickReaction(message, emoji);
    this.closeReactionsList();
  }

  reactorAvatarUrl(reaction: MessageReaction): string | undefined {
    return this.isOtherReaction(reaction) ? this.activeConversation()?.avatarUrl : this.authService.currentAvatarUrl() ?? undefined;
  }

  reactorName(reaction: MessageReaction): string {
    if (this.isOtherReaction(reaction)) return this.activeConversation()?.username ?? '';
    return this.authService.currentFullName() || this.authService.currentUsername() || '';
  }

  /** Distinct emoji among a message's reactions, in first-used order — shown together in one badge alongside the total count. */
  uniqueReactionEmojis(message: ChatMessage): string[] {
    return [...new Set(message.reactions.map((r) => r.emoji))];
  }

  /** Opens the "Đang trả lời [username]" bar above the composer for this message. */
  startReply(message: ChatMessage, event: Event): void {
    event.stopPropagation();
    this.replyTarget.set(message);
  }

  cancelReply(): void {
    this.replyTarget.set(null);
  }

  /** One-line quoted snippet for the reply bar/preview — a placeholder for non-text messages, same wording as previewOf(). */
  replySnippet(message: ChatMessage | { content?: string; type: ChatMessage['type'] }): string {
    if (message.type === 'Image') return 'Đã gửi một ảnh.';
    if (message.type === 'Video') return 'Đã gửi một video.';
    if (message.type === 'Audio') return 'Đã gửi một đoạn ghi âm.';
    return message.content ?? '';
  }

  /** Whether the quoted message in a reply preview was sent by the caller — decides the "bạn" vs. sender's name label. */
  isReplyToMine(replyTo: NonNullable<ChatMessage['replyTo']>): boolean {
    return replyTo.senderUserId !== this.activeConversation()?.otherUserId;
  }

  /** The "[sender] đã trả lời bạn" label shown above a reply's quoted preview. */
  replyLabel(message: ChatMessage): string {
    const replyTo = message.replyTo;
    if (!replyTo) return '';

    const otherName = this.activeConversation()?.username ?? '';
    const senderIsMe = this.isMine(message);
    const quotedIsMe = this.isReplyToMine(replyTo);

    if (senderIsMe && !quotedIsMe) return `Bạn đã trả lời ${otherName}`;
    if (!senderIsMe && quotedIsMe) return `${otherName} đã trả lời bạn`;
    if (senderIsMe && quotedIsMe) return 'Bạn đã trả lời chính mình';
    return `${otherName} đã trả lời chính mình`;
  }

  @HostListener('document:click')
  closePickers(): void {
    this.emojiPickerOpen.set(false);
    this.reactionPickerMessageId.set(null);
    this.reactionsListMessage.set(null);
  }

  toggleEmojiPicker(): void {
    this.emojiPickerOpen.update((v) => !v);
  }

  addEmoji(emoji: string): void {
    this.draft.update((v) => v + emoji);
    this.emojiPickerOpen.set(false);
  }

  private scrollToBottom(): void {
    const el = this.messagesContainer()?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }

  /** Preview line for the conversation list — a placeholder for non-text messages, matching what the sender's own bubble shows. */
  previewOf(c: ConversationSummary): string {
    if (c.lastMessageType === 'Image') return 'Đã gửi một ảnh.';
    if (c.lastMessageType === 'Video') return 'Đã gửi một video.';
    if (c.lastMessageType === 'Audio') return 'Đã gửi một đoạn ghi âm.';
    return c.lastMessagePreview ?? '';
  }

  /** "N phút/giờ/ngày/tuần trước" — same style as elsewhere, with a "trước" suffix to match the reference screenshot's activity line. */
  relativeTime(iso: string): string {
    const mins = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 60000));
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 1) return isVi ? 'Vừa xong' : 'Just now';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    if (days < 7) return isVi ? `${days} ngày` : `${days}d`;
    const weeks = Math.round(days / 7);
    return isVi ? `${weeks} tuần` : `${weeks}w`;
  }
}
