import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Observable, Subject, map, tap } from 'rxjs';

import { ApiResponse } from '../api';

export interface CreatedStory {
  id: string;
  userId: string;
}

interface CreateStoryApiResponse {
  id: string;
  userId: string;
}

/**
 * Opens the "create story" dialog (same MatDialog chrome as CreatePostService)
 * and performs the actual upload. A story is a single image or video (capped
 * at 15s, enforced client-side before upload) plus an optional caption and AI
 * label — no carousel, no alt text, no cross-posting section.
 */
@Injectable({ providedIn: 'root' })
export class CreateStoryService {
  private readonly http = inject(HttpClient);
  private readonly dialog = inject(MatDialog);

  /** Emits every time a story is successfully created, so the home feed's tray can refresh itself. */
  private readonly storyCreatedSource = new Subject<CreatedStory>();
  readonly storyCreated$ = this.storyCreatedSource.asObservable();

  async open(): Promise<void> {
    // Lazy-imported so the dialog's code isn't in the initial bundle for pages that never open it.
    const { CreateStoryModalComponent } = await import('./create-story-modal.component');

    this.dialog.open(CreateStoryModalComponent, {
      maxWidth: '95vw',
      maxHeight: '95vh',
      autoFocus: false,
      panelClass: 'create-post-dialog-panel'
    });
  }

  createStory(file: File, caption: string, isAiGenerated: boolean): Observable<CreatedStory> {
    const formData = new FormData();
    formData.append('File', file, file.name);
    if (caption.trim()) {
      formData.append('Caption', caption.trim());
    }
    formData.append('IsAiGenerated', String(isAiGenerated));

    return this.http.post<ApiResponse<CreateStoryApiResponse>>('/api/stories', formData).pipe(
      map((res) => res.data!),
      tap((created) => this.storyCreatedSource.next(created))
    );
  }
}
