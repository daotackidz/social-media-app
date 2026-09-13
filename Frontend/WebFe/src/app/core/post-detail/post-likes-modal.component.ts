import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';

import { ProfileService } from '../../pages/profile/services/profile.service';
import { PostLikeUser } from './post-detail.models';
import { PostDetailService } from './post-detail.service';

export interface PostLikesDialogData {
  postId: string;
  /** When set, lists that comment's (or reply's) likers instead of the post's. */
  commentId?: string;
}

/** The "Lượt thích" popup — who liked a post, with a Follow/Following button per row. */
@Component({
  selector: 'app-post-likes-modal',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './post-likes-modal.component.html',
  styleUrl: './post-likes-modal.component.scss'
})
export class PostLikesModalComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<PostLikesModalComponent>);
  private readonly postDetailService = inject(PostDetailService);
  private readonly profileService = inject(ProfileService);

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly users = signal<PostLikeUser[]>([]);
  readonly hasMore = signal(false);
  readonly loadingMore = signal(false);

  private readonly pendingUsernames = signal<Set<string>>(new Set());

  constructor(@Inject(MAT_DIALOG_DATA) private readonly data: PostLikesDialogData) {}

  ngOnInit(): void {
    this.fetchPage(0, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.users.set(items);
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

    this.fetchPage(this.users().length, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.users.update((current) => [...current, ...items]);
        this.hasMore.set(hasMore);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false)
    });
  }

  private fetchPage(skip: number, take: number) {
    return this.data.commentId
      ? this.postDetailService.getCommentLikes(this.data.postId, this.data.commentId, skip, take)
      : this.postDetailService.getLikes(this.data.postId, skip, take);
  }

  isPending(username: string): boolean {
    return this.pendingUsernames().has(username);
  }

  /** Follow (or send a request) / unfollow — same toggle as the profile page's button. */
  toggleFollow(user: PostLikeUser): void {
    if (this.isPending(user.username)) return;

    this.pendingUsernames.update((set) => new Set(set).add(user.username));
    const request$ = user.isFollowing ? this.profileService.unfollow(user.username) : this.profileService.follow(user.username);

    request$.subscribe({
      next: (result) => {
        this.users.update((list) => list.map((u) => (u.username === user.username ? { ...u, isFollowing: result.isFollowing } : u)));
        this.clearPending(user.username);
      },
      error: () => this.clearPending(user.username)
    });
  }

  private clearPending(username: string): void {
    this.pendingUsernames.update((set) => {
      const next = new Set(set);
      next.delete(username);
      return next;
    });
  }
}
