import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';

import { TranslatePipe } from '../i18n/translate.pipe';
import { StoryViewerInfo } from './story.models';
import { StoryViewerService } from './story-viewer.service';

export interface StoryViewersDialogData {
  storyId: string;
}

/** Owner-only "who viewed this story" popup — same list chrome as PostLikesModalComponent, with a small heart instead of a Follow button. */
@Component({
  selector: 'app-story-viewers-modal',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './story-viewers-modal.component.html',
  styleUrl: './story-viewers-modal.component.scss'
})
export class StoryViewersModalComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<StoryViewersModalComponent>);
  private readonly storyViewerService = inject(StoryViewerService);

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly viewers = signal<StoryViewerInfo[]>([]);
  readonly hasMore = signal(false);
  readonly loadingMore = signal(false);

  constructor(@Inject(MAT_DIALOG_DATA) private readonly data: StoryViewersDialogData) {}

  ngOnInit(): void {
    this.storyViewerService.getViewers(this.data.storyId, 0, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.viewers.set(items);
        this.hasMore.set(hasMore);
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

  loadMore(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    this.loadingMore.set(true);

    this.storyViewerService.getViewers(this.data.storyId, this.viewers().length, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.viewers.update((current) => [...current, ...items]);
        this.hasMore.set(hasMore);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false)
    });
  }
}
