import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, Subject, map } from 'rxjs';

import { ApiResponse } from '../api';
import { StoryDetail, StoryViewerInfo } from './story.models';

interface StoryItemDto {
  id: string;
  userId: string;
  username: string;
  avatarUrl?: string | null;
  mediaUrl: string;
  mediaType: 'image' | 'video';
  caption: string;
  isAiGenerated: boolean;
  createdDate: string;
  likeCount: number;
  viewCount: number;
  isLiked: boolean;
  isOwnStory: boolean;
}

interface StoryViewerDto {
  userId: string;
  username: string;
  fullName?: string | null;
  avatarUrl?: string | null;
  viewedDate: string;
  liked: boolean;
}

function toStory(dto: StoryItemDto): StoryDetail {
  return {
    id: dto.id,
    userId: dto.userId,
    username: dto.username,
    avatarUrl: dto.avatarUrl ?? undefined,
    mediaUrl: dto.mediaUrl,
    mediaType: dto.mediaType,
    caption: dto.caption,
    isAiGenerated: dto.isAiGenerated,
    createdDate: dto.createdDate,
    likeCount: dto.likeCount,
    viewCount: dto.viewCount,
    isLiked: dto.isLiked,
    isOwnStory: dto.isOwnStory
  };
}

function toViewer(dto: StoryViewerDto): StoryViewerInfo {
  return {
    userId: dto.userId,
    username: dto.username,
    fullName: dto.fullName ?? undefined,
    avatarUrl: dto.avatarUrl ?? undefined,
    viewedDate: dto.viewedDate,
    liked: dto.liked
  };
}

/**
 * Opens the full-screen story viewer overlay and backs its API calls. Mirrors
 * Backend/Social.WebApi/Controllers/StoriesController.cs.
 */
@Injectable({ providedIn: 'root' })
export class StoryViewerService {
  private readonly http = inject(HttpClient);
  private readonly dialog = inject(MatDialog);

  /** Emits an owner's userId once one of their stories has been marked viewed — lets the home tray gray out that ring without a full refetch. */
  private readonly storyViewedSource = new Subject<string>();
  readonly storyViewed$ = this.storyViewedSource.asObservable();

  /**
   * Opens the viewer starting at ownerUserIds[startIndex] — the full ring order from the tray is
   * passed along so next/prev can move seamlessly from one owner's last story to the next owner's
   * first. initialStoryId, when given, jumps straight to that story within the starting owner's
   * reel instead of their first/last one (e.g. opening the exact story a notification is about).
   */
  async open(ownerUserIds: string[], startIndex: number, initialStoryId?: string): Promise<void> {
    const { StoryViewerModalComponent } = await import('./story-viewer-modal.component');

    this.dialog.open(StoryViewerModalComponent, {
      width: '100vw',
      height: '100vh',
      maxWidth: '100vw',
      maxHeight: '100vh',
      autoFocus: false,
      panelClass: 'story-viewer-dialog-panel',
      data: { ownerUserIds, startIndex, initialStoryId }
    });
  }

  /**
   * Opens the viewer over an already-fetched flat list of stories (the archive page's own grid) —
   * next/prev simply walks this list instead of crossing between different owners' reels, and no
   * further fetching happens. Used for the "Kho lưu trữ" (archive) page, since those can include
   * expired stories that the normal owner-reel endpoints deliberately exclude.
   */
  async openFlat(stories: StoryDetail[], startIndex: number): Promise<void> {
    const { StoryViewerModalComponent } = await import('./story-viewer-modal.component');

    this.dialog.open(StoryViewerModalComponent, {
      width: '100vw',
      height: '100vh',
      maxWidth: '100vw',
      maxHeight: '100vh',
      autoFocus: false,
      panelClass: 'story-viewer-dialog-panel',
      data: { flatStories: stories, startIndex }
    });
  }

  /** Resolves a single story's owner (a notification only names the story itself) then opens the viewer on that owner's reel, jumped straight to it. */
  async openStory(storyId: string): Promise<void> {
    this.getById(storyId).subscribe({
      next: (story) => this.open([story.userId], 0, storyId),
      error: () => undefined
    });
  }

  getById(storyId: string): Observable<StoryDetail> {
    return this.http.get<ApiResponse<StoryItemDto>>(`/api/stories/${storyId}`).pipe(map((res) => toStory(res.data!)));
  }

  async openViewers(storyId: string): Promise<void> {
    const { StoryViewersModalComponent } = await import('./story-viewers-modal.component');

    this.dialog.open(StoryViewersModalComponent, {
      maxWidth: '95vw',
      maxHeight: '80vh',
      autoFocus: false,
      panelClass: 'post-likes-dialog-panel',
      data: { storyId }
    });
  }

  getByUser(userId: string): Observable<StoryDetail[]> {
    return this.http.get<ApiResponse<StoryItemDto[]>>(`/api/stories/by-user/${userId}`).pipe(map((res) => (res.data ?? []).map(toStory)));
  }

  /** Every story the signed-in user has ever posted, regardless of expiry, newest first — "Kho lưu trữ" on their own profile. */
  getArchive(skip = 0, take = 30): Observable<{ items: StoryDetail[]; hasMore: boolean }> {
    return this.http.get<ApiResponse<{ items: StoryItemDto[]; hasMore: boolean }>>('/api/stories/archive', { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return { items: data.items.map(toStory), hasMore: data.hasMore };
      })
    );
  }

  markViewed(storyId: string, ownerUserId: string): void {
    this.http.post<ApiResponse<null>>(`/api/stories/${storyId}/view`, {}).subscribe(() => this.storyViewedSource.next(ownerUserId));
  }

  toggleLike(storyId: string): Observable<{ liked: boolean; likeCount: number }> {
    return this.http.post<ApiResponse<{ liked: boolean; likeCount: number }>>(`/api/stories/${storyId}/like`, {}).pipe(map((res) => res.data!));
  }

  reply(storyId: string, content: string): Observable<void> {
    return this.http.post<ApiResponse<unknown>>(`/api/stories/${storyId}/reply`, { content }).pipe(map(() => undefined));
  }

  getViewers(storyId: string, skip = 0, take = 30): Observable<{ items: StoryViewerInfo[]; hasMore: boolean }> {
    return this.http
      .get<ApiResponse<{ items: StoryViewerDto[]; hasMore: boolean }>>(`/api/stories/${storyId}/viewers`, { params: { skip, take } })
      .pipe(
        map((res) => {
          const data = res.data ?? { items: [], hasMore: false };
          return { items: data.items.map(toViewer), hasMore: data.hasMore };
        })
      );
  }
}
