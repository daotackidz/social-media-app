import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../core/api';
import { FeedPage, FeedPost, StoryItem, SuggestedUser, SuggestionReason } from '../models/feed.models';

/** Raw shapes returned by FeedController — mirrors Backend/Social.Data/Model/Response/Feed/*.cs. */
interface FeedPostDto {
  id: string;
  username: string;
  verified: boolean;
  avatarUrl?: string | null;
  imageUrls: string[];
  caption: string;
  likeCount: number;
  commentCount: number;
  createdDate: string;
}

interface FeedStoryDto {
  userId: string;
  username: string;
  avatarUrl?: string | null;
  viewed: boolean;
}

interface FeedSuggestedUserDto {
  userId: string;
  username: string;
  fullName?: string | null;
  avatarUrl?: string | null;
  reason: SuggestionReason;
  reasonUsername?: string | null;
}

/** Deterministic fallback tile color/initial for a username — the API doesn't send these, only a real avatarUrl when one exists. */
const AVATAR_PALETTE = ['#f5a623', '#111827', '#2563eb', '#0ea5e9', '#9333ea', '#dc2626', '#059669', '#7c2d12', '#334155', '#4c1d95'];

function avatarColorFor(username: string): string {
  let hash = 0;
  for (let i = 0; i < username.length; i++) hash = (hash * 31 + username.charCodeAt(i)) >>> 0;
  return AVATAR_PALETTE[hash % AVATAR_PALETTE.length];
}

function avatarInitialFor(username: string): string {
  return (username || '?').charAt(0).toUpperCase();
}

function toSuggestedUser(u: FeedSuggestedUserDto): SuggestedUser {
  return {
    id: u.userId,
    username: u.username,
    fullName: u.fullName ?? undefined,
    reason: u.reason,
    reasonName: u.reasonUsername ?? undefined,
    avatarUrl: u.avatarUrl ?? undefined,
    avatarColor: avatarColorFor(u.username)
  };
}

/**
 * Each block on the home feed (stories / posts / suggestions) has its own
 * method here, all backed by FeedController: posts and stories are paged
 * (skip/take), suggestions are a single take-N fetch of real users from the
 * follow graph (followed-you / mutual-connection / newest-account fallback).
 */
@Injectable({ providedIn: 'root' })
export class FeedService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/feed';

  getStories(skip = 0, take = 20): Observable<FeedPage<StoryItem>> {
    return this.http.get<ApiResponse<{ items: FeedStoryDto[]; hasMore: boolean }>>(`${this.baseUrl}/stories`, { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return {
          hasMore: data.hasMore,
          items: data.items.map(
            (s): StoryItem => ({
              userId: s.userId,
              username: s.username,
              avatarUrl: s.avatarUrl ?? undefined,
              avatarColor: avatarColorFor(s.username),
              avatarInitial: avatarInitialFor(s.username),
              viewed: s.viewed
            })
          )
        };
      })
    );
  }

  getPosts(skip = 0, take = 10): Observable<FeedPage<FeedPost>> {
    return this.http.get<ApiResponse<{ items: FeedPostDto[]; hasMore: boolean }>>(`${this.baseUrl}/posts`, { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return {
          hasMore: data.hasMore,
          items: data.items.map(
            (p): FeedPost => ({
              id: p.id,
              username: p.username,
              verified: p.verified,
              avatarUrl: p.avatarUrl ?? undefined,
              avatarColor: avatarColorFor(p.username),
              avatarInitial: avatarInitialFor(p.username),
              imageUrls: p.imageUrls ?? [],
              caption: p.caption,
              likeCount: p.likeCount,
              commentCount: p.commentCount,
              createdDate: p.createdDate
            })
          )
        };
      })
    );
  }

  getInlineSuggestions(take = 5): Observable<SuggestedUser[]> {
    return this.getSuggestions(take);
  }

  getPanelSuggestions(take = 5): Observable<SuggestedUser[]> {
    return this.getSuggestions(take);
  }

  private getSuggestions(take: number): Observable<SuggestedUser[]> {
    return this.http
      .get<ApiResponse<FeedSuggestedUserDto[]>>(`${this.baseUrl}/suggestions`, { params: { take } })
      .pipe(map((res) => (res.data ?? []).map(toSuggestedUser)));
  }
}
