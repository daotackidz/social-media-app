import { Component, ElementRef, HostListener, OnDestroy, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

import { LanguageService } from '../i18n/language.service';
import { AuthService } from '../../services/auth.service';
import { ChatMessage, ConversationSummary } from '../../pages/messages/models/messages.models';
import { MessagesService } from '../../pages/messages/services/messages.service';
import { ChatWidgetService } from './chat-widget.service';

/** How often the open mini-thread re-polls while the widget panel is visible. */
const THREAD_POLL_MS = 4000;
/** How often the (collapsed) conversation list refreshes in the background, for the unread dot / avatar stack. */
const LIST_POLL_MS = 20000;

/** Route prefixes where the floating widget stays hidden — the full messages page already owns this UI there. */
const HIDDEN_PREFIXES = ['/messages', '/login', '/register', '/verify-otp', '/forgot-password', '/reset-password'];

/**
 * A Messenger-style floating chat widget mounted globally (see app.html): a small pill
 * in the corner of every page that opens a compact popup — conversation list, or one
 * open thread — without leaving the current page. Reuses the same MessagesService/
 * backend as the full /messages page; this is purely an additional, lighter-weight UI
 * on top of it, so it intentionally skips reactions/replies/voice notes to stay small.
 */
@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [],
  templateUrl: './chat-widget.component.html',
  styleUrl: './chat-widget.component.scss'
})
export class ChatWidgetComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly messagesService = inject(MessagesService);
  private readonly router = inject(Router);
  private readonly languageService = inject(LanguageService);
  private readonly chatWidgetService = inject(ChatWidgetService);

  private readonly currentPath = signal(window.location.pathname);
  readonly visible = computed(() => this.authService.isLoggedIn() && !HIDDEN_PREFIXES.some((p) => this.currentPath().startsWith(p)));

  readonly panelOpen = signal(false);
  readonly conversations = signal<ConversationSummary[]>([]);
  readonly conversationsLoading = signal(true);

  readonly activeConversation = signal<ConversationSummary | null>(null);
  readonly messages = signal<ChatMessage[]>([]);
  readonly threadLoading = signal(false);

  readonly draft = signal('');
  readonly sending = signal(false);
  readonly emojiPickerOpen = signal(false);
  readonly commonEmojis = [
    '😂', '😮', '😞', '😡', '👏', '🔥', '🎉', '💯',
    '❤️', '🤣', '🤗', '🤔', '☺️', '😊',
    '😍', '😘', '😭', '😅', '😁', '😉', '🙌', '🙏',
    '👍', '👎', '👋', '💪', '✨', '🎂', '😎', '🥳'
  ];

  /** Up to 3 most-recently-active conversations, shown as the overlapping avatar stack on the collapsed trigger. */
  readonly previewAvatars = computed(() => this.conversations().slice(0, 3));
  readonly hasUnread = computed(() => this.conversations().some((c) => c.hasUnread));

  private readonly messagesContainer = viewChild<ElementRef<HTMLElement>>('widgetMessages');

  private threadPollHandle?: ReturnType<typeof setInterval>;
  private listPollHandle?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe((e) => {
      this.currentPath.set(e.urlAfterRedirects);
    });

    this.loadConversations();
    this.listPollHandle = setInterval(() => this.loadConversations(), LIST_POLL_MS);

    this.chatWidgetService.openRequests$.subscribe((username) => this.openWithUsername(username));
  }

  ngOnDestroy(): void {
    if (this.listPollHandle) clearInterval(this.listPollHandle);
    this.stopThreadPolling();
  }

  /** Opens (or creates) the thread with this username and shows it in the widget — the "Nhắn tin" button on someone's profile page routes here via ChatWidgetService. */
  private openWithUsername(username: string): void {
    this.panelOpen.set(true);
    this.activeConversation.set(null);
    this.messages.set([]);
    this.threadLoading.set(true);

    this.messagesService.startConversation(username).subscribe({
      next: (conversation) => this.openThread(conversation),
      error: () => this.threadLoading.set(false)
    });
  }

  private loadConversations(): void {
    this.messagesService.getConversations(0, 10).subscribe({
      next: ({ items }) => {
        this.conversations.set(items);
        this.conversationsLoading.set(false);
      },
      error: () => this.conversationsLoading.set(false)
    });
  }

  togglePanel(): void {
    this.panelOpen.update((v) => !v);
    if (this.panelOpen()) this.loadConversations();
  }

  closePanel(): void {
    this.panelOpen.set(false);
    this.emojiPickerOpen.set(false);
    this.backToList();
  }

  openThread(c: ConversationSummary): void {
    this.activeConversation.set(c);
    this.messages.set([]);
    this.threadLoading.set(true);
    this.emojiPickerOpen.set(false);

    this.messagesService.getMessages(c.id, 0, 20).subscribe({
      next: ({ items }) => {
        this.messages.set([...items].reverse());
        this.threadLoading.set(false);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => this.threadLoading.set(false)
    });

    this.messagesService.markRead(c.id).subscribe();
    this.conversations.update((list) => list.map((x) => (x.id === c.id ? { ...x, hasUnread: false } : x)));
    this.threadPollHandle = setInterval(() => this.pollMessages(c.id), THREAD_POLL_MS);
  }

  backToList(): void {
    this.stopThreadPolling();
    this.activeConversation.set(null);
    this.messages.set([]);
    this.draft.set('');
  }

  private stopThreadPolling(): void {
    if (this.threadPollHandle) clearInterval(this.threadPollHandle);
    this.threadPollHandle = undefined;
  }

  private pollMessages(id: string): void {
    this.messagesService.getMessages(id, 0, 20).subscribe({
      next: ({ items }) => {
        const newest = [...items].reverse();
        const known = new Set(this.messages().map((m) => m.id));
        const added = newest.filter((m) => !known.has(m.id));
        if (added.length > 0) {
          this.messages.update((list) => [...list, ...added]);
          this.messagesService.markRead(id).subscribe();
          queueMicrotask(() => this.scrollToBottom());
        }
      },
      error: () => undefined
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
    const c = this.activeConversation();
    const content = this.draft().trim();
    if (!c || !content || this.sending()) return;

    this.sending.set(true);
    this.messagesService.sendMessage(c.id, content).subscribe({
      next: (message) => {
        this.messages.update((list) => [...list, message]);
        this.draft.set('');
        this.sending.set(false);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => this.sending.set(false)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0);
    input.value = '';
    const c = this.activeConversation();
    if (!file || !c || this.sending()) return;

    this.sending.set(true);
    this.messagesService.sendMessage(c.id, '', file).subscribe({
      next: (message) => {
        this.messages.update((list) => [...list, message]);
        this.sending.set(false);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: () => this.sending.set(false)
    });
  }

  /** Opens the same conversation full-screen at /messages/:id and collapses the widget. */
  expandThread(): void {
    const id = this.activeConversation()?.id;
    this.closePanel();
    if (id) this.router.navigate(['/messages', id]);
  }

  /** Opens the full /messages page and collapses the widget. */
  expandList(): void {
    this.closePanel();
    this.router.navigate(['/messages']);
  }

  isMine(message: ChatMessage): boolean {
    return message.senderUserId !== this.activeConversation()?.otherUserId;
  }

  isSeenIndicator(message: ChatMessage): boolean {
    const list = this.messages();
    if (list.length === 0 || list[list.length - 1].id !== message.id) return false;
    if (!this.isMine(message)) return false;

    const readAt = this.activeConversation()?.otherLastReadAt;
    if (!readAt) return false;
    return new Date(readAt).getTime() >= new Date(message.createdDate).getTime();
  }

  @HostListener('document:click')
  closeEmojiPicker(): void {
    this.emojiPickerOpen.set(false);
  }

  toggleEmojiPicker(event: Event): void {
    event.stopPropagation();
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

  /** Preview line for the conversation list — a placeholder for non-text messages, same wording as the full page. */
  previewOf(c: ConversationSummary): string {
    if (c.lastMessageType === 'Image') return 'Đã gửi một ảnh.';
    if (c.lastMessageType === 'Video') return 'Đã gửi một video.';
    if (c.lastMessageType === 'Audio') return 'Đã gửi một đoạn ghi âm.';
    return c.lastMessagePreview ?? '';
  }

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
