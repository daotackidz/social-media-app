import { Component, ElementRef, Inject, OnDestroy, OnInit, inject, signal, viewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../i18n/language.service';
import { TranslatePipe } from '../i18n/translate.pipe';
import { StoryDetail } from './story.models';
import { StoryViewerService } from './story-viewer.service';

export interface StoryViewerDialogData {
  /** The tray's full ring order — lets next/prev move seamlessly from one owner's last story to the next owner's first. Omitted in flat mode (see flatStories). */
  ownerUserIds?: string[];
  startIndex: number;
  /** Jumps straight to this story within the starting owner's reel instead of their first/last one (e.g. opening the exact story a notification is about). */
  initialStoryId?: string;
  /** Archive mode: an already-fetched flat list to play through directly (no owner-reel fetching, no owner crossover on next/prev) — used for "Kho lưu trữ", which can include expired stories the normal reel endpoints exclude. */
  flatStories?: StoryDetail[];
}

const IMAGE_DURATION_MS = 5000;
const MAX_VIDEO_DURATION_MS = 15000;

/**
 * Full-screen story viewer: one owner's reel at a time, auto-advancing through
 * segmented progress bars (Instagram-style), with next/prev crossing over into
 * neighboring owners' reels once the current one runs out.
 */
@Component({
  selector: 'app-story-viewer-modal',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './story-viewer-modal.component.html',
  styleUrl: './story-viewer-modal.component.scss'
})
export class StoryViewerModalComponent implements OnInit, OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<StoryViewerModalComponent>);
  private readonly storyViewerService = inject(StoryViewerService);
  private readonly languageService = inject(LanguageService);

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly stories = signal<StoryDetail[]>([]);
  readonly activeIndex = signal(0);
  readonly ownerIndex = signal(0);

  readonly paused = signal(false);
  readonly muted = signal(true);
  /** 0..1 fraction of the active story's duration elapsed — drives the segmented progress bars. */
  readonly progress = signal(0);
  /** Whether the active story's own <img>/<video> has actually finished loading — gates its fade-in and the boxed spinner, and whether the auto-advance timer may run yet. */
  readonly mediaLoaded = signal(false);

  readonly liked = signal(false);
  readonly likeCount = signal(0);
  private likeRequestPending = false;

  readonly replyDraft = signal('');
  readonly replySending = signal(false);
  readonly replySent = signal(false);

  private readonly storyVideo = viewChild<ElementRef<HTMLVideoElement>>('storyVideo');

  private readonly ownerUserIds: string[];
  /** Consumed on the very first loadOwner() call only, then cleared — later owner switches always enter at first/last. */
  private initialStoryId: string | undefined;
  /** Set in flat (archive) mode — next/prev then just walks this list instead of crossing owners. */
  private readonly isFlat: boolean;
  private readonly flatStartIndex: number;
  private rafId: number | null = null;
  private durationMs = IMAGE_DURATION_MS;
  private segmentStartPerf = 0;
  private elapsedBeforePauseMs = 0;
  /** Stories already POSTed as viewed this session, so navigating back and forth doesn't refire the request needlessly. */
  private readonly viewedStoryIds = new Set<string>();

  constructor(@Inject(MAT_DIALOG_DATA) data: StoryViewerDialogData) {
    this.isFlat = !!data.flatStories;
    this.ownerUserIds = data.ownerUserIds ?? [];
    this.flatStartIndex = data.startIndex;
    this.ownerIndex.set(this.ownerUserIds.length ? Math.max(0, Math.min(data.startIndex, this.ownerUserIds.length - 1)) : 0);
    this.initialStoryId = data.initialStoryId;

    if (data.flatStories) this.stories.set(data.flatStories);
  }

  get currentStory(): StoryDetail | null {
    return this.stories()[this.activeIndex()] ?? null;
  }

  ngOnInit(): void {
    if (this.isFlat) {
      const stories = this.stories();
      this.activeIndex.set(Math.max(0, Math.min(this.flatStartIndex, stories.length - 1)));
      this.loading.set(false);
      this.onActiveStoryChanged();
      return;
    }

    this.loadOwner(this.ownerIndex(), 'first', 'forward');
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  close(): void {
    this.dialogRef.close();
  }

  /** Loads ownerUserIds[index]'s reel; owners with no active stories left (expired/deleted since the tray loaded) are skipped in the given direction. */
  private loadOwner(index: number, enter: 'first' | 'last', direction: 'forward' | 'backward'): void {
    if (index < 0 || index >= this.ownerUserIds.length) {
      this.close();
      return;
    }

    this.loading.set(true);
    this.loadError.set(false);
    this.stopTimer();

    this.storyViewerService.getByUser(this.ownerUserIds[index]).subscribe({
      next: (items) => {
        if (items.length === 0) {
          this.loadOwner(direction === 'forward' ? index + 1 : index - 1, enter, direction);
          return;
        }

        let startAt = enter === 'first' ? 0 : items.length - 1;
        if (this.initialStoryId) {
          const targetIndex = items.findIndex((s) => s.id === this.initialStoryId);
          if (targetIndex >= 0) startAt = targetIndex;
          this.initialStoryId = undefined;
        }
        this.ownerIndex.set(index);
        this.stories.set(items);
        this.activeIndex.set(startAt);
        this.loading.set(false);
        this.onActiveStoryChanged();

        // Quietly warms the browser cache for the rest of this owner's reel so next/prev feels
        // instant — doesn't block or gate anything, just fires the same network fetch early.
        for (const [i, item] of items.entries()) {
          if (i !== startAt) this.warmCache(item);
        }
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      }
    });
  }

  private warmCache(story: StoryDetail): void {
    if (story.mediaType === 'image') {
      new Image().src = story.mediaUrl;
    } else {
      const video = document.createElement('video');
      video.preload = 'auto';
      video.muted = true;
      video.src = story.mediaUrl;
    }
  }

  private onActiveStoryChanged(): void {
    const story = this.currentStory;
    if (!story) return;

    this.liked.set(story.isLiked);
    this.likeCount.set(story.likeCount);
    this.replyDraft.set('');
    this.replySent.set(false);
    this.paused.set(false);
    this.stopTimer();
    this.progress.set(0);

    // The boxed spinner shows until the template's own <img>/<video> fires (load)/(loadeddata) —
    // see onMediaReady() — which is also what starts the auto-advance timer, so the progress bar
    // can never race ahead of a still-blank frame while a heavy photo/video is downloading.
    this.mediaLoaded.set(false);

    if (!this.viewedStoryIds.has(story.id)) {
      this.viewedStoryIds.add(story.id);
      this.storyViewerService.markViewed(story.id, story.userId);
    }
  }

  /** Fired by the active story's own (load)/(loadeddata) event once it's actually visible. */
  onMediaReady(): void {
    if (this.mediaLoaded()) return;
    this.mediaLoaded.set(true);

    const story = this.currentStory;
    if (!story) return;

    if (story.mediaType === 'image') {
      this.startTimer(IMAGE_DURATION_MS);
      return;
    }

    const video = this.storyVideo()?.nativeElement;
    const durationMs = video && isFinite(video.duration) ? video.duration * 1000 : MAX_VIDEO_DURATION_MS;
    this.startTimer(Math.min(durationMs, MAX_VIDEO_DURATION_MS));
    video?.play().catch(() => undefined);
  }

  private startTimer(durationMs: number): void {
    this.stopTimer();
    this.durationMs = durationMs;
    this.elapsedBeforePauseMs = 0;
    this.segmentStartPerf = performance.now();
    this.progress.set(0);
    this.rafId = requestAnimationFrame(this.tick);
  }

  private stopTimer(): void {
    if (this.rafId !== null) {
      cancelAnimationFrame(this.rafId);
      this.rafId = null;
    }
  }

  private readonly tick = (): void => {
    if (!this.paused()) {
      const elapsed = this.elapsedBeforePauseMs + (performance.now() - this.segmentStartPerf);
      const frac = Math.min(1, elapsed / this.durationMs);
      this.progress.set(frac);
      if (frac >= 1) {
        this.nextStory();
        return;
      }
    }
    this.rafId = requestAnimationFrame(this.tick);
  };

  togglePause(): void {
    const wasPaused = this.paused();
    if (wasPaused) {
      this.segmentStartPerf = performance.now();
    } else {
      this.elapsedBeforePauseMs += performance.now() - this.segmentStartPerf;
    }
    this.paused.set(!wasPaused);

    const video = this.storyVideo()?.nativeElement;
    if (video) {
      if (wasPaused) video.play().catch(() => undefined);
      else video.pause();
    }
  }

  toggleMute(): void {
    this.muted.update((v) => !v);
  }

  /** Pauses the auto-advance while composing a reply, without flipping the visible pause icon. */
  private suspendForInput(): void {
    if (this.paused()) return;
    this.elapsedBeforePauseMs += performance.now() - this.segmentStartPerf;
    this.paused.set(true);
    this.storyVideo()?.nativeElement.pause();
  }

  private resumeFromInput(): void {
    if (this.replyDraft().trim()) return;
    this.segmentStartPerf = performance.now();
    this.paused.set(false);
    this.storyVideo()?.nativeElement.play().catch(() => undefined);
  }

  onReplyFocus(): void {
    this.suspendForInput();
  }

  onReplyBlur(): void {
    this.resumeFromInput();
  }

  onReplyInput(event: Event): void {
    this.replyDraft.set((event.target as HTMLTextAreaElement).value);
  }

  sendReply(): void {
    const story = this.currentStory;
    const content = this.replyDraft().trim();
    if (!story || !content || this.replySending()) return;

    this.replySending.set(true);
    this.storyViewerService.reply(story.id, content).subscribe({
      next: () => {
        this.replySending.set(false);
        this.replySent.set(true);
        this.replyDraft.set('');
        setTimeout(() => this.replySent.set(false), 2000);
      },
      error: () => this.replySending.set(false)
    });
  }

  /** Optimistic like/unlike, mirroring the post/comment toggle pattern. */
  toggleLike(): void {
    const story = this.currentStory;
    if (!story || this.likeRequestPending) return;

    const wasLiked = this.liked();
    this.liked.set(!wasLiked);
    this.likeCount.update((c) => Math.max(0, c + (wasLiked ? -1 : 1)));
    this.likeRequestPending = true;

    this.storyViewerService.toggleLike(story.id).subscribe({
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

  openViewers(): void {
    const story = this.currentStory;
    if (story) this.storyViewerService.openViewers(story.id);
  }

  nextStory(): void {
    const stories = this.stories();
    const index = this.activeIndex();
    if (index < stories.length - 1) {
      this.activeIndex.set(index + 1);
      this.onActiveStoryChanged();
    } else if (this.isFlat) {
      // End of the archive list — nothing to cross into, just close like reaching the tray's last owner.
      this.close();
    } else {
      this.loadOwner(this.ownerIndex() + 1, 'first', 'forward');
    }
  }

  prevStory(): void {
    const index = this.activeIndex();
    if (index > 0) {
      this.activeIndex.set(index - 1);
      this.onActiveStoryChanged();
    } else if (!this.isFlat && this.ownerIndex() > 0) {
      this.loadOwner(this.ownerIndex() - 1, 'last', 'backward');
    } else {
      // Already at the very first story overall — restart it, matching Instagram's own behavior.
      this.onActiveStoryChanged();
    }
  }

  /** "N phút/giờ/ngày" — same style as the post detail overlay. */
  relativeTime(iso: string): string {
    const mins = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 60000));
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    return isVi ? `${days} ngày` : `${days}d`;
  }
}
