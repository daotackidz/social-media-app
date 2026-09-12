import { Component, Input, inject, signal } from '@angular/core';

import { LanguageService } from '../../../../core/i18n/language.service';
import { FeedPost } from '../../models/feed.models';

@Component({
  selector: 'app-feed-post',
  standalone: true,
  templateUrl: './feed-post.component.html',
  styleUrl: './feed-post.component.scss'
})
export class FeedPostComponent {
  @Input({ required: true }) post!: FeedPost;

  private readonly languageService = inject(LanguageService);

  readonly liked = signal(false);
  readonly saved = signal(false);

  /** Which image is showing when a post has more than one — the slide/dot indicators below the photo. */
  readonly activeImageIndex = signal(0);

  /** Horizontal drag-to-swipe state — not a signal, doesn't need to trigger change detection. */
  private dragStartX: number | null = null;
  private dragStartY: number | null = null;
  private dragHandled = false;
  private static readonly SWIPE_THRESHOLD_PX = 40;

  toggleLike(): void {
    this.liked.update((v) => !v);
  }

  toggleSave(): void {
    this.saved.update((v) => !v);
  }

  prevImage(): void {
    const count = this.post.imageUrls.length;
    if (count < 2) return;
    this.activeImageIndex.update((i) => (i - 1 + count) % count);
  }

  nextImage(): void {
    const count = this.post.imageUrls.length;
    if (count < 2) return;
    this.activeImageIndex.update((i) => (i + 1) % count);
  }

  /** Drag-to-swipe on the photo itself — pointer events cover mouse and touch alike. */
  onImagePointerDown(event: PointerEvent): void {
    if (this.post.imageUrls.length < 2) return;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.dragHandled = false;
  }

  onImagePointerMove(event: PointerEvent): void {
    if (this.dragStartX === null || this.dragStartY === null || this.dragHandled) return;

    const deltaX = event.clientX - this.dragStartX;
    const deltaY = event.clientY - this.dragStartY;
    // Ignore mostly-vertical gestures so the page can still scroll normally over the photo.
    if (Math.abs(deltaY) > Math.abs(deltaX)) return;

    if (Math.abs(deltaX) >= FeedPostComponent.SWIPE_THRESHOLD_PX) {
      this.dragHandled = true;
      if (deltaX < 0) this.nextImage();
      else this.prevImage();
    }
  }

  onImagePointerUp(): void {
    this.dragStartX = null;
    this.dragStartY = null;
    this.dragHandled = false;
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
    return this.compact(this.post.likeCount + (this.liked() ? 1 : 0));
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
