import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';

import { ApiResponse } from '../api';
import { AppNotification, NotificationActor, NotificationKind } from './notification.models';

interface NotificationActorDto {
  userId: string;
  username: string;
  avatarUrl?: string | null;
}

interface NotificationDto {
  id: string;
  type: NotificationKind;
  actors: NotificationActorDto[];
  totalActorCount: number;
  commentPreview?: string | null;
  postId?: string | null;
  storyId?: string | null;
  thumbnailUrl?: string | null;
  isRead: boolean;
  createdDate: string;
  isFollowingActor?: boolean | null;
}

function toActor(dto: NotificationActorDto): NotificationActor {
  return { userId: dto.userId, username: dto.username, avatarUrl: dto.avatarUrl ?? undefined };
}

function toNotification(dto: NotificationDto): AppNotification {
  return {
    id: dto.id,
    type: dto.type,
    actors: dto.actors.map(toActor),
    totalActorCount: dto.totalActorCount,
    commentPreview: dto.commentPreview ?? undefined,
    postId: dto.postId ?? undefined,
    storyId: dto.storyId ?? undefined,
    thumbnailUrl: dto.thumbnailUrl ?? undefined,
    isRead: dto.isRead,
    createdDate: dto.createdDate,
    isFollowingActor: dto.isFollowingActor ?? undefined
  };
}

/** Backs the bell-icon popup. Mirrors Backend/Social.WebApi/Controllers/NotificationsController.cs. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);

  /** Drives the red dot on the bell icon — refreshed on load and after marking things read. */
  readonly unreadCount = signal(0);

  refreshUnreadCount(): void {
    this.http.get<ApiResponse<{ count: number }>>('/api/notifications/unread-count').subscribe({
      next: (res) => this.unreadCount.set(res.data?.count ?? 0),
      error: () => undefined
    });
  }

  getNotifications(skip = 0, take = 20): Observable<{ items: AppNotification[]; hasMore: boolean }> {
    return this.http.get<ApiResponse<{ items: NotificationDto[]; hasMore: boolean }>>('/api/notifications', { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return { items: data.items.map(toNotification), hasMore: data.hasMore };
      })
    );
  }

  markRead(id: string): Observable<void> {
    return this.http.post<ApiResponse<null>>(`/api/notifications/${id}/read`, {}).pipe(
      map(() => undefined),
      tap(() => this.unreadCount.update((c) => Math.max(0, c - 1)))
    );
  }

  markAllRead(): Observable<void> {
    return this.http.post<ApiResponse<null>>('/api/notifications/read-all', {}).pipe(
      map(() => undefined),
      tap(() => this.unreadCount.set(0))
    );
  }
}
