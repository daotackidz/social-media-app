import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../core/api';
import { avatarColorFor, avatarInitialFor } from '../utils/avatar-color';
import { FollowResult, ProfileHighlight, ProfilePost, ProfileTab, UserProfileSummary } from '../models/profile.models';

interface ProfileApiResponse {
  id: string;
  username: string;
  fullName: string;
  bio?: string | null;
  websiteUrl?: string | null;
  avatarUrl?: string | null;
  verified: boolean;
  isCurrentUser: boolean;
  isFollowing: boolean;
  followedByUsername?: string | null;
  postsCount: number;
  followersCount: number;
  followingCount: number;
}

interface ProfileHighlightApiResponse {
  id: string;
  title: string;
  coverUrl?: string | null;
}

interface ProfilePostApiResponse {
  id: string;
  type: ProfilePost['type'];
  coverUrl?: string | null;
  likeCount: number;
  commentCount: number;
}

/**
 * HTTP-backed profile data source — matches Backend/Social.WebApi/Controllers/ProfileViewController.cs.
 * Component-facing shapes stay the same as when this was mocked (see profile.models.ts);
 * only this file needed to change to switch from `of(...)` to real HttpClient calls.
 */
@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/profile';

  getProfile(username: string): Observable<UserProfileSummary> {
    return this.http.get<ApiResponse<ProfileApiResponse>>(`${this.baseUrl}/${username}`).pipe(
      map((res) => this.toProfileSummary(res.data!))
    );
  }

  getHighlights(username: string): Observable<ProfileHighlight[]> {
    return this.http.get<ApiResponse<ProfileHighlightApiResponse[]>>(`${this.baseUrl}/${username}/highlights`).pipe(
      map((res) => (res.data ?? []).map((h) => this.toHighlight(h)))
    );
  }

  getPosts(username: string, tab: ProfileTab = 'posts'): Observable<ProfilePost[]> {
    return this.http.get<ApiResponse<ProfilePostApiResponse[]>>(`${this.baseUrl}/${username}/posts`, {
      params: { tab }
    }).pipe(
      map((res) => (res.data ?? []).map((p) => this.toPost(p, username)))
    );
  }

  follow(username: string): Observable<FollowResult> {
    return this.http.post<ApiResponse<FollowResult>>(`${this.baseUrl}/${username}/follow`, {}).pipe(
      map((res) => res.data!)
    );
  }

  unfollow(username: string): Observable<FollowResult> {
    return this.http.post<ApiResponse<FollowResult>>(`${this.baseUrl}/${username}/unfollow`, {}).pipe(
      map((res) => res.data!)
    );
  }

  private toProfileSummary(data: ProfileApiResponse): UserProfileSummary {
    return {
      username: data.username,
      fullName: data.fullName,
      bio: data.bio ?? '',
      websiteUrl: data.websiteUrl ?? undefined,
      avatarUrl: data.avatarUrl ?? undefined,
      avatarColor: avatarColorFor(data.username),
      avatarInitial: avatarInitialFor(data.fullName || data.username),
      verified: data.verified,
      isCurrentUser: data.isCurrentUser,
      isFollowing: data.isFollowing,
      followedByUsername: data.followedByUsername ?? undefined,
      stats: {
        postsCount: data.postsCount,
        followersCount: data.followersCount,
        followingCount: data.followingCount
      }
    };
  }

  private toHighlight(data: ProfileHighlightApiResponse): ProfileHighlight {
    return {
      id: data.id,
      title: data.title,
      coverUrl: data.coverUrl ?? undefined,
      avatarColor: avatarColorFor(data.id),
      avatarInitial: avatarInitialFor(data.title)
    };
  }

  private toPost(data: ProfilePostApiResponse, username: string): ProfilePost {
    return {
      id: data.id,
      type: data.type,
      coverUrl: data.coverUrl ?? undefined,
      avatarColor: avatarColorFor(username + data.id),
      likeCount: data.likeCount,
      commentCount: data.commentCount
    };
  }
}
