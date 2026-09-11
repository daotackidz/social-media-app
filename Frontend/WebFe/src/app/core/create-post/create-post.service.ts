import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../api';

export interface CreatedPost {
  id: string;
  type: 'image' | 'video' | 'carousel';
  coverUrl?: string;
}

interface CreatePostApiResponse {
  id: string;
  type: 'image' | 'video' | 'carousel';
  coverUrl?: string | null;
}

/**
 * Opens the "create post" dialog (via Angular Material's MatDialog — no
 * hand-rolled overlay/backdrop) and performs the actual upload. Sizing is
 * left to the dialog component's own responsive classes rather than a fixed
 * width/height here, so it isn't pinned to one viewport size.
 */
@Injectable({ providedIn: 'root' })
export class CreatePostService {
  private readonly http = inject(HttpClient);
  private readonly dialog = inject(MatDialog);

  async open(): Promise<void> {
    // Lazy-imported so the dialog's code isn't in the initial bundle for
    // pages that never open it.
    const { CreatePostModalComponent } = await import('./create-post-modal.component');

    this.dialog.open(CreatePostModalComponent, {
      maxWidth: '95vw',
      maxHeight: '95vh',
      autoFocus: false,
      panelClass: 'create-post-dialog-panel'
    });
  }

  createPost(files: File[], caption: string): Observable<CreatedPost> {
    const formData = new FormData();
    if (caption.trim()) {
      formData.append('Caption', caption.trim());
    }
    for (const file of files) {
      formData.append('Files', file, file.name);
    }

    return this.http.post<ApiResponse<CreatePostApiResponse>>('/api/posts', formData).pipe(
      map((res) => {
        const data = res.data!;
        return { id: data.id, type: data.type, coverUrl: data.coverUrl ?? undefined };
      })
    );
  }
}
