import { Component, ElementRef, HostListener, Inject, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../i18n/language.service';
import { PostComment, PostDetail } from './post-detail.models';
import { PostDetailService } from './post-detail.service';

export interface PostDetailDialogData {
  postId: string;
}

/** Which root comment (and, when replying to a reply, whose name to prefix) the draft below the thread will be posted under. Undefined = a new top-level comment. */
interface ReplyTarget {
  rootCommentId: string;
  mentionUsername: string;
}

@Component({
  selector: 'app-post-detail-modal',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './post-detail-modal.component.html',
  styleUrl: './post-detail-modal.component.scss'
})
export class PostDetailModalComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<PostDetailModalComponent>);
  private readonly postDetailService = inject(PostDetailService);
  private readonly languageService = inject(LanguageService);

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly post = signal<PostDetail | null>(null);
  readonly activeImageIndex = signal(0);

  /** Seeded from the fetched post's isLiked/likeCount once it loads, then kept in sync with the server on every toggle. */
  readonly liked = signal(false);
  readonly likeCount = signal(0);
  private likeRequestPending = false;

  readonly saved = signal(false);

  readonly commentDraft = signal('');
  readonly submittingComment = signal(false);
  readonly loadingMoreComments = signal(false);
  readonly replyTarget = signal<ReplyTarget | null>(null);

  private readonly pendingCommentLikes = signal<Set<string>>(new Set());
  private readonly loadingReplies = signal<Set<string>>(new Set());

  readonly likeCountDisplay = computed(() => this.likeCount());

  readonly emojiPickerOpen = signal(false);
  readonly commonEmojis = [
    '😂', '😮', '😞', '😡', '👏', '🔥', '🎉', '💯',
    '❤️', '🤣', '🤗', '🤔', '☺️', '😊',
    '😍', '😘', '😭', '😅', '😁', '😉', '🙌', '🙏',
    '👍', '👎', '👋', '💪', '✨', '🎂', '😎', '🥳',
    '😢', '😱', '🤩', '🤯', '🙄', '😴', '🤤', '🤢',
    '💔', '💕', '⭐', '✅', '🌸', '🍀', '☀️', '🌙'
  ];
  private readonly commentInput = viewChild<ElementRef<HTMLTextAreaElement>>('commentInput');
  private readonly emojiPicker = viewChild<ElementRef<HTMLElement>>('emojiPicker');

  constructor(@Inject(MAT_DIALOG_DATA) private readonly data: PostDetailDialogData) {}

  ngOnInit(): void {
    this.postDetailService.getDetail(this.data.postId).subscribe({
      next: (detail) => {
        this.post.set(detail);
        this.liked.set(detail.isLiked);
        this.likeCount.set(detail.likeCount);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set(true);
        this.loading.set(false);
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  /** Per-URL "has this actually finished downloading" flag — every image/poster starts hidden behind a spinner sized to its own frame and fades in once its (load) event fires, instead of the whole modal waiting on one big spinner. */
  private readonly loadedMedia = signal<Set<string>>(new Set());

  isMediaLoaded(url: string | null | undefined): boolean {
    return !url || this.loadedMedia().has(url);
  }

  /** Also used as the (error) handler — a broken image should still clear its own spinner rather than spin forever. */
  onMediaLoad(url: string): void {
    if (this.loadedMedia().has(url)) return;
    this.loadedMedia.update((set) => new Set(set).add(url));
  }

  /** Optimistic like/unlike — flips the UI immediately, then reconciles with (or reverts to match) the server's response. */
  toggleLike(): void {
    const p = this.post();
    if (!p || this.likeRequestPending) return;

    const wasLiked = this.liked();
    this.liked.set(!wasLiked);
    this.likeCount.update((c) => Math.max(0, c + (wasLiked ? -1 : 1)));
    this.likeRequestPending = true;

    this.postDetailService.toggleLike(p.id).subscribe({
      next: ({ liked, likeCount }) => {
        this.liked.set(liked);
        this.likeCount.set(likeCount);
        this.likeRequestPending = false;
      },
      error: () => {
        this.liked.set(wasLiked);
        this.likeCount.update((c) => Math.max(0, c + (wasLiked ? 1 : -1)));
        this.likeRequestPending = false;
      }
    });
  }

  toggleSave(): void {
    this.saved.update((v) => !v);
  }

  openLikes(): void {
    const p = this.post();
    if (p) this.postDetailService.openLikes(p.id);
  }

  openCommentLikes(commentId: string): void {
    const p = this.post();
    if (p) this.postDetailService.openCommentLikes(p.id, commentId);
  }

  prevImage(): void {
    this.activeImageIndex.update((i) => Math.max(0, i - 1));
  }

  nextImage(): void {
    const count = this.post()?.imageUrls.length ?? 0;
    this.activeImageIndex.update((i) => Math.min(count - 1, i + 1));
  }

  onCommentInput(event: Event): void {
    this.commentDraft.set((event.target as HTMLTextAreaElement).value);
  }

  /** Infinite-scroll trigger for the comment thread — loads the next page of top-level comments a bit before the panel actually bottoms out. */
  onCommentsScroll(event: Event): void {
    const el = event.target as HTMLElement;
    if (el.scrollHeight - el.scrollTop - el.clientHeight < 150) {
      this.loadMoreComments();
    }
  }

  loadMoreComments(): void {
    const p = this.post();
    if (!p || !p.commentsHasMore || this.loadingMoreComments()) return;

    this.loadingMoreComments.set(true);
    this.postDetailService.getComments(p.id, p.comments.length, 10).subscribe({
      next: ({ items, hasMore }) => {
        this.post.update((current) => (current ? { ...current, comments: [...current.comments, ...items], commentsHasMore: hasMore } : current));
        this.loadingMoreComments.set(false);
      },
      error: () => this.loadingMoreComments.set(false)
    });
  }

  /** Expands a comment's replies (fetching them the first time only) or, once loaded, just toggles "Ẩn câu trả lời" without refetching. */
  toggleReplies(comment: PostComment): void {
    const p = this.post();
    if (!p) return;

    if (comment.replies !== undefined) {
      this.updateComment(comment.id, (c) => ({ ...c, repliesExpanded: !c.repliesExpanded }));
      return;
    }
    if (this.loadingReplies().has(comment.id)) return;

    this.loadingReplies.update((set) => new Set(set).add(comment.id));
    this.postDetailService.getReplies(p.id, comment.id, 0, 20).subscribe({
      next: ({ items, hasMore }) => {
        this.updateComment(comment.id, (c) => ({ ...c, replies: items, repliesExpanded: true, repliesHasMore: hasMore }));
        this.clearLoadingReplies(comment.id);
      },
      error: () => this.clearLoadingReplies(comment.id)
    });
  }

  loadMoreReplies(comment: PostComment): void {
    const p = this.post();
    if (!p || this.loadingReplies().has(comment.id)) return;

    this.loadingReplies.update((set) => new Set(set).add(comment.id));
    this.postDetailService.getReplies(p.id, comment.id, comment.replies?.length ?? 0, 20).subscribe({
      next: ({ items, hasMore }) => {
        this.updateComment(comment.id, (c) => ({ ...c, replies: [...(c.replies ?? []), ...items], repliesHasMore: hasMore }));
        this.clearLoadingReplies(comment.id);
      },
      error: () => this.clearLoadingReplies(comment.id)
    });
  }

  isLoadingReplies(commentId: string): boolean {
    return this.loadingReplies().has(commentId);
  }

  private clearLoadingReplies(commentId: string): void {
    this.loadingReplies.update((set) => {
      const next = new Set(set);
      next.delete(commentId);
      return next;
    });
  }

  /** Optimistic like/unlike on a comment or reply — mirrors the post-level toggle. */
  toggleCommentLike(comment: PostComment): void {
    const p = this.post();
    if (!p || this.pendingCommentLikes().has(comment.id)) return;

    const wasLiked = comment.isLiked;
    this.pendingCommentLikes.update((set) => new Set(set).add(comment.id));
    this.updateComment(comment.id, (c) => ({ ...c, isLiked: !wasLiked, likeCount: Math.max(0, c.likeCount + (wasLiked ? -1 : 1)) }));

    this.postDetailService.toggleCommentLike(p.id, comment.id).subscribe({
      next: ({ liked, likeCount }) => {
        this.updateComment(comment.id, (c) => ({ ...c, isLiked: liked, likeCount }));
        this.clearPendingCommentLike(comment.id);
      },
      error: () => {
        this.updateComment(comment.id, (c) => ({ ...c, isLiked: wasLiked, likeCount: Math.max(0, c.likeCount + (wasLiked ? 1 : -1)) }));
        this.clearPendingCommentLike(comment.id);
      }
    });
  }

  isCommentLikePending(commentId: string): boolean {
    return this.pendingCommentLikes().has(commentId);
  }

  private clearPendingCommentLike(commentId: string): void {
    this.pendingCommentLikes.update((set) => {
      const next = new Set(set);
      next.delete(commentId);
      return next;
    });
  }

  /** Focuses the input for a reply — rootComment is always the top-level comment; mentionUsername may be a reply's author when replying to a reply (kept flat under the same root, like Instagram). */
  replyTo(rootComment: PostComment, mentionUsername?: string): void {
    this.replyTarget.set({ rootCommentId: rootComment.id, mentionUsername: mentionUsername ?? rootComment.username });
    this.commentDraft.set(`@${mentionUsername ?? rootComment.username} `);

    queueMicrotask(() => {
      const textarea = this.commentInput()?.nativeElement;
      textarea?.focus();
      textarea?.setSelectionRange(this.commentDraft().length, this.commentDraft().length);
    });
  }

  cancelReply(): void {
    this.replyTarget.set(null);
    this.commentDraft.set('');
  }

  submitComment(): void {
    const content = this.commentDraft().trim();
    const p = this.post();
    if (!content || !p || this.submittingComment()) return;

    const target = this.replyTarget();
    this.submittingComment.set(true);

    this.postDetailService.addComment(p.id, content, target?.rootCommentId).subscribe({
      next: (comment: PostComment) => {
        if (target) {
          this.updateComment(target.rootCommentId, (c) => ({
            ...c,
            replies: [...(c.replies ?? []), comment],
            repliesExpanded: true,
            replyCount: c.replyCount + 1
          }));
        } else {
          this.post.update((current) => (current ? { ...current, comments: [...current.comments, comment] } : current));
        }

        this.post.update((current) => (current ? { ...current, commentCount: current.commentCount + 1 } : current));
        this.replyTarget.set(null);
        this.commentDraft.set('');
        this.submittingComment.set(false);
      },
      error: () => this.submittingComment.set(false)
    });
  }

  toggleEmojiPicker(): void {
    this.emojiPickerOpen.update((v) => !v);
  }

  addEmoji(emoji: string): void {
    const textarea = this.commentInput()?.nativeElement;
    const start = textarea?.selectionStart ?? this.commentDraft().length;
    const end = textarea?.selectionEnd ?? this.commentDraft().length;
    const value = this.commentDraft();
    const next = value.slice(0, start) + emoji + value.slice(end);
    this.commentDraft.set(next);
    this.emojiPickerOpen.set(false);

    if (textarea) {
      queueMicrotask(() => {
        const cursor = start + emoji.length;
        textarea.focus();
        textarea.setSelectionRange(cursor, cursor);
      });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.emojiPickerOpen()) return;
    const target = event.target as Node;
    if (this.emojiPicker()?.nativeElement.contains(target)) return;
    this.emojiPickerOpen.set(false);
  }

  /** Recursively updates one comment or reply anywhere in the loaded thread by id, without disturbing sibling array/object identities. */
  private updateComment(commentId: string, updater: (c: PostComment) => PostComment): void {
    const updateList = (list: PostComment[]): PostComment[] =>
      list.map((c) => {
        if (c.id === commentId) return updater(c);
        if (c.replies) return { ...c, replies: updateList(c.replies) };
        return c;
      });

    this.post.update((current) => (current ? { ...current, comments: updateList(current.comments) } : current));
  }

  /** "N phút/giờ/ngày/tuần" — same style as the feed card, extended with weeks for older posts. */
  relativeTime(iso: string): string {
    const mins = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 60000));
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    if (days < 7) return isVi ? `${days} ngày` : `${days}d`;
    const weeks = Math.round(days / 7);
    return isVi ? `${weeks} tuần` : `${weeks}w`;
  }

  fullDate(iso: string): string {
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'long', year: 'numeric' }).format(new Date(iso));
  }
}
