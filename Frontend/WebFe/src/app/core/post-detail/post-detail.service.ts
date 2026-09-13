import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../api';
import { PostComment, PostDetail, PostLikeUser } from './post-detail.models';

interface PostCommentDto {
  id: string;
  username: string;
  avatarUrl?: string | null;
  content: string;
  createdDate: string;
  likeCount: number;
  isLiked: boolean;
  replyCount: number;
}

interface PostDetailDto {
  id: string;
  username: string;
  verified: boolean;
  avatarUrl?: string | null;
  imageUrls: string[];
  videoUrl?: string | null;
  posterUrl?: string | null;
  caption: string;
  edited: boolean;
  likeCount: number;
  isLiked: boolean;
  commentCount: number;
  createdDate: string;
  comments: PostCommentDto[];
  commentsHasMore: boolean;
}

interface PostLikeDto {
  liked: boolean;
  likeCount: number;
}

interface CommentLikeDto {
  liked: boolean;
  likeCount: number;
}

interface PostLikeUserDto {
  userId: string;
  username: string;
  fullName?: string | null;
  avatarUrl?: string | null;
  isCurrentUser: boolean;
  isFollowing: boolean;
}

function toLikeUser(dto: PostLikeUserDto): PostLikeUser {
  return {
    userId: dto.userId,
    username: dto.username,
    fullName: dto.fullName ?? undefined,
    avatarUrl: dto.avatarUrl ?? undefined,
    isCurrentUser: dto.isCurrentUser,
    isFollowing: dto.isFollowing
  };
}

function toComment(dto: PostCommentDto): PostComment {
  return {
    id: dto.id,
    username: dto.username,
    avatarUrl: dto.avatarUrl ?? undefined,
    content: dto.content,
    createdDate: dto.createdDate,
    likeCount: dto.likeCount,
    isLiked: dto.isLiked,
    replyCount: dto.replyCount
  };
}

/**
 * Opens the "post detail" dialog (image/video + caption + comment thread) —
 * shared by the profile grid (clicking a thumbnail) and the home feed
 * (clicking the comment icon). Mirrors Backend/Social.WebApi/Controllers/PostsController.cs's
 * GetById/GetComments/AddComment.
 */
@Injectable({ providedIn: 'root' })
export class PostDetailService {
  private readonly http = inject(HttpClient);
  private readonly dialog = inject(MatDialog);

  async open(postId: string): Promise<void> {
    // Lazy-imported so the modal's code isn't in the initial bundle for pages that never open it.
    const { PostDetailModalComponent } = await import('./post-detail-modal.component');

    this.dialog.open(PostDetailModalComponent, {
      maxWidth: '95vw',
      maxHeight: '95vh',
      autoFocus: false,
      panelClass: 'post-detail-dialog-panel',
      data: { postId }
    });
  }

  getDetail(postId: string): Observable<PostDetail> {
    return this.http.get<ApiResponse<PostDetailDto>>(`/api/posts/${postId}`).pipe(
      map((res) => {
        const dto = res.data!;
        return {
          id: dto.id,
          username: dto.username,
          verified: dto.verified,
          avatarUrl: dto.avatarUrl ?? undefined,
          imageUrls: dto.imageUrls ?? [],
          videoUrl: dto.videoUrl ?? undefined,
          posterUrl: dto.posterUrl ?? undefined,
          caption: dto.caption,
          edited: dto.edited,
          likeCount: dto.likeCount,
          isLiked: dto.isLiked,
          commentCount: dto.commentCount,
          createdDate: dto.createdDate,
          comments: (dto.comments ?? []).map(toComment),
          commentsHasMore: dto.commentsHasMore
        };
      })
    );
  }

  /** A page of top-level comments (replies excluded) — used to load more as the comment panel scrolls past the initial page from getDetail(). */
  getComments(postId: string, skip = 0, take = 10): Observable<{ items: PostComment[]; hasMore: boolean }> {
    return this.http.get<ApiResponse<{ items: PostCommentDto[]; hasMore: boolean }>>(`/api/posts/${postId}/comments`, { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return { items: data.items.map(toComment), hasMore: data.hasMore };
      })
    );
  }

  /** A page of replies under one top-level comment. */
  getReplies(postId: string, commentId: string, skip = 0, take = 20): Observable<{ items: PostComment[]; hasMore: boolean }> {
    return this.http
      .get<ApiResponse<{ items: PostCommentDto[]; hasMore: boolean }>>(`/api/posts/${postId}/comments/${commentId}/replies`, { params: { skip, take } })
      .pipe(
        map((res) => {
          const data = res.data ?? { items: [], hasMore: false };
          return { items: data.items.map(toComment), hasMore: data.hasMore };
        })
      );
  }

  /** parentId, when given, replies to that top-level comment (or, if it's itself a reply, its own parent — kept flat like Instagram). */
  addComment(postId: string, content: string, parentId?: string): Observable<PostComment> {
    return this.http
      .post<ApiResponse<PostCommentDto>>(`/api/posts/${postId}/comments`, { content, parentId })
      .pipe(map((res) => toComment(res.data!)));
  }

  /** Likes the post if not already liked by the caller, unlikes it otherwise. Returns the resulting state and the post's updated count. */
  toggleLike(postId: string): Observable<{ liked: boolean; likeCount: number }> {
    return this.http.post<ApiResponse<PostLikeDto>>(`/api/posts/${postId}/like`, {}).pipe(map((res) => res.data!));
  }

  /** Likes the comment (or reply) if not already liked by the caller, unlikes it otherwise. */
  toggleCommentLike(postId: string, commentId: string): Observable<{ liked: boolean; likeCount: number }> {
    return this.http.post<ApiResponse<CommentLikeDto>>(`/api/posts/${postId}/comments/${commentId}/like`, {}).pipe(map((res) => res.data!));
  }

  /** Opens the "Lượt thích" (likes list) popup for a post. */
  async openLikes(postId: string): Promise<void> {
    // Lazy-imported so the modal's code isn't in the initial bundle for pages that never open it.
    const { PostLikesModalComponent } = await import('./post-likes-modal.component');

    this.dialog.open(PostLikesModalComponent, {
      maxWidth: '95vw',
      maxHeight: '80vh',
      autoFocus: false,
      panelClass: 'post-likes-dialog-panel',
      data: { postId }
    });
  }

  /** Opens the same "Lượt thích" popup, scoped to one comment (or reply)'s likers instead of the post's. */
  async openCommentLikes(postId: string, commentId: string): Promise<void> {
    const { PostLikesModalComponent } = await import('./post-likes-modal.component');

    this.dialog.open(PostLikesModalComponent, {
      maxWidth: '95vw',
      maxHeight: '80vh',
      autoFocus: false,
      panelClass: 'post-likes-dialog-panel',
      data: { postId, commentId }
    });
  }

  getLikes(postId: string, skip = 0, take = 30): Observable<{ items: PostLikeUser[]; hasMore: boolean }> {
    return this.http.get<ApiResponse<{ items: PostLikeUserDto[]; hasMore: boolean }>>(`/api/posts/${postId}/likes`, { params: { skip, take } }).pipe(
      map((res) => {
        const data = res.data ?? { items: [], hasMore: false };
        return { items: data.items.map(toLikeUser), hasMore: data.hasMore };
      })
    );
  }

  /** Same shape as getLikes, scoped to one comment (or reply)'s likers. */
  getCommentLikes(postId: string, commentId: string, skip = 0, take = 30): Observable<{ items: PostLikeUser[]; hasMore: boolean }> {
    return this.http
      .get<ApiResponse<{ items: PostLikeUserDto[]; hasMore: boolean }>>(`/api/posts/${postId}/comments/${commentId}/likes`, { params: { skip, take } })
      .pipe(
        map((res) => {
          const data = res.data ?? { items: [], hasMore: false };
          return { items: data.items.map(toLikeUser), hasMore: data.hasMore };
        })
      );
  }
}
