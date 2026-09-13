import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../../core/i18n/language.service';
import { PostDetailService } from '../../../../core/post-detail/post-detail.service';
import { FeedPost } from '../../models/feed.models';

@Component({
  selector: 'app-feed-post',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './feed-post.component.html',
  styleUrl: './feed-post.component.scss'
})
export class FeedPostComponent implements OnInit {
  @Input({ required: true }) post!: FeedPost;

  private readonly languageService = inject(LanguageService);
  private readonly postDetailService = inject(PostDetailService);

  /** Seeded from post.isLiked/likeCount in ngOnInit, then kept in sync with the server on every toggle. */
  readonly liked = signal(false);
  readonly likeCount = signal(0);
  private likeRequestPending = false;

  readonly saved = signal(false);

  ngOnInit(): void {
    this.liked.set(this.post.isLiked);
    this.likeCount.set(this.post.likeCount);
  }

  /** Per-URL "has this actually finished downloading" flag — each image/poster starts hidden behind a spinner sized to its own frame and fades in on its (load) event, instead of a jarring pop-in once the bytes arrive. */
  private readonly loadedMedia = signal<Set<string>>(new Set());

  isMediaLoaded(url: string | null | undefined): boolean {
    return !url || this.loadedMedia().has(url);
  }

  /** Also used as the (error) handler — a broken image should still clear its own spinner rather than spin forever. */
  onMediaLoad(url: string): void {
    if (this.loadedMedia().has(url)) return;
    this.loadedMedia.update((set) => new Set(set).add(url));
  }

  /** Which image is showing when a post has more than one — the slide/dot indicators below the photo. */
  readonly activeImageIndex = signal(0);

  /** True while a drag is in progress — disables the CSS transition (so the track follows the pointer 1:1) and switches the cursor to "grabbing". */
  readonly isDragging = signal(false);

  /** Live drag offset in px, added on top of the current slide's position while dragging. */
  readonly dragOffsetPx = signal(0);

  /** translateX combining the settled slide position (in %) with the live drag offset (in px). */
  readonly trackTransform = computed(() => `translateX(calc(${-this.activeImageIndex() * 100}% + ${this.dragOffsetPx()}px))`);

  private dragStartX: number | null = null;
  private dragStartY: number | null = null;
  /** Set once the gesture's direction is clear, so a vertical scroll can't be hijacked mid-drag. */
  private dragAxis: 'x' | 'y' | null = null;
  private static readonly SWIPE_THRESHOLD_PX = 40;
  /** How far past the first/last image the track is allowed to rubber-band, as a fraction of the raw drag distance. */
  private static readonly EDGE_RESISTANCE = 3;

  /** Optimistic like/unlike — flips the UI immediately, then reconciles with (or reverts to match) the server's response. */
  toggleLike(): void {
    if (this.likeRequestPending) return;

    const wasLiked = this.liked();
    this.liked.set(!wasLiked);
    this.likeCount.update((c) => Math.max(0, c + (wasLiked ? -1 : 1)));
    this.likeRequestPending = true;

    this.postDetailService.toggleLike(this.post.id).subscribe({
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

  openDetail(): void {
    this.postDetailService.open(this.post.id);
  }

  openLikes(): void {
    this.postDetailService.openLikes(this.post.id);
  }

  /** Clamped, not wrapping — matches Instagram's own carousel (stops at the first/last image). */
  prevImage(): void {
    this.activeImageIndex.update((i) => Math.max(0, i - 1));
  }

  nextImage(): void {
    this.activeImageIndex.update((i) => Math.min(this.post.imageUrls.length - 1, i + 1));
  }

  /**
   * Drag-to-swipe on the photo itself — pointer events cover mouse and touch alike.
   * Deliberately doesn't call setPointerCapture: capturing to this wrapper retargets
   * pointerup's hit-test to it, which makes some browsers decide the prev/next/dot
   * buttons' mousedown and mouseup "targets" no longer match and drop their click
   * entirely. pointerleave below covers the case a fast drag exits the wrapper.
   */
  onImagePointerDown(event: PointerEvent): void {
    if (this.post.imageUrls.length < 2) return;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.dragAxis = null;
    this.isDragging.set(true);
  }

  onImagePointerMove(event: PointerEvent): void {
    if (this.dragStartX === null || this.dragStartY === null) return;

    const deltaX = event.clientX - this.dragStartX;
    const deltaY = event.clientY - this.dragStartY;

    // Decide once, a few px in, whether this gesture is a horizontal swipe or a vertical
    // page scroll — committing early avoids fighting the browser's own scroll gesture.
    if (this.dragAxis === null) {
      if (Math.abs(deltaX) < 6 && Math.abs(deltaY) < 6) return;
      this.dragAxis = Math.abs(deltaX) > Math.abs(deltaY) ? 'x' : 'y';
      if (this.dragAxis === 'y') {
        this.endDrag();
        return;
      }
    }
    if (this.dragAxis !== 'x') return;

    const count = this.post.imageUrls.length;
    const draggingPastStart = this.activeImageIndex() === 0 && deltaX > 0;
    const draggingPastEnd = this.activeImageIndex() === count - 1 && deltaX < 0;
    this.dragOffsetPx.set(draggingPastStart || draggingPastEnd ? deltaX / FeedPostComponent.EDGE_RESISTANCE : deltaX);
  }

  onImagePointerUp(): void {
    if (this.dragStartX === null) return;
    const offset = this.dragOffsetPx();
    const wasHorizontal = this.dragAxis === 'x';
    this.endDrag();

    if (wasHorizontal && Math.abs(offset) >= FeedPostComponent.SWIPE_THRESHOLD_PX) {
      if (offset < 0) this.nextImage();
      else this.prevImage();
    }
  }

  /** Resets drag state and hands the track back to the transition (which snaps it to the settled slide). */
  private endDrag(): void {
    this.dragStartX = null;
    this.dragStartY = null;
    this.dragAxis = null;
    this.isDragging.set(false);
    this.dragOffsetPx.set(0);
  }

  get relativeTime(): string {
    const mins = Math.max(0, Math.round((Date.now() - new Date(this.post.createdDate).getTime()) / 60000));
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    return isVi ? `${days} ngày` : `${days}d`;
  }

  get likesDisplay(): string {
    return this.compact(this.likeCount());
  }

  get commentsCountDisplay(): string {
    return this.compact(this.post.commentCount);
  }

  /** "264,7K" / "1,5K" style — matches the compact counter next to the like/comment icons in the new design. */
  private compact(value: number): string {
    const decimalSeparator = this.languageService.lang() === 'vi' ? ',' : '.';
    const scale: [number, string][] = [
      [1_000_000_000, 'B'],
      [1_000_000, 'M'],
      [1_000, 'K']
    ];
    for (const [threshold, suffix] of scale) {
      if (value >= threshold) {
        return (value / threshold).toFixed(1).replace(/\.0$/, '').replace('.', decimalSeparator) + suffix;
      }
    }
    return String(value);
  }
}
