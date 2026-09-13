import { NgTemplateOutlet } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { PostDetailService } from '../../../../core/post-detail/post-detail.service';
import { StoryViewerService } from '../../../../core/story-viewer/story-viewer.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { LanguageService } from '../../../../core/i18n/language.service';
import { NotificationService } from '../../../../core/notifications/notification.service';
import { AppNotification } from '../../../../core/notifications/notification.models';
import { ProfileService } from '../../../profile/services/profile.service';
import { FollowRequestItem } from '../../../profile/models/profile.models';

type FilterKey = 'all' | 'comments' | 'likes';

const COMMENT_TYPES = new Set(['CommentPost', 'CommentStory', 'ReplyComment']);
const LIKE_TYPES = new Set(['LikePost', 'LikeStory', 'LikeComment']);

/**
 * The bell-icon fly-out — same panel chrome as SearchPopupComponent. Pending
 * follow requests are pulled live from ProfileService (not from the notification
 * feed) so that section always exactly matches "who's still waiting", disappearing
 * the moment a request is approved/rejected elsewhere.
 */
@Component({
  selector: 'app-notifications-popup',
  standalone: true,
  imports: [NgTemplateOutlet, TranslatePipe],
  templateUrl: './notifications-popup.component.html',
  styleUrl: './notifications-popup.component.scss'
})
export class NotificationsPopupComponent implements OnInit {
  @Output() readonly closed = new EventEmitter<void>();

  private readonly notificationService = inject(NotificationService);
  private readonly profileService = inject(ProfileService);
  private readonly postDetailService = inject(PostDetailService);
  private readonly storyViewerService = inject(StoryViewerService);
  private readonly router = inject(Router);
  private readonly languageService = inject(LanguageService);

  readonly filters: { key: FilterKey; labelKey: string }[] = [
    { key: 'all', labelKey: 'notifications.filterAll' },
    { key: 'likes', labelKey: 'notifications.filterLikes' },
    { key: 'comments', labelKey: 'notifications.filterComments' }
  ];
  readonly activeFilter = signal<FilterKey>('all');

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly notifications = signal<AppNotification[]>([]);
  readonly hasMore = signal(false);
  readonly loadingMore = signal(false);

  readonly followRequests = signal<FollowRequestItem[]>([]);

  private readonly pendingFollowActors = signal<Set<string>>(new Set());

  ngOnInit(): void {
    this.profileService.getFollowRequests().subscribe((requests) => this.followRequests.set(requests));

    this.notificationService.getNotifications(0, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.notifications.set(items);
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
    this.closed.emit();
  }

  setFilter(key: FilterKey): void {
    this.activeFilter.set(key);
  }

  /** FollowRequest-type rows are represented by the pinned section (live pending list) instead, so they're excluded here. */
  get visibleNotifications(): AppNotification[] {
    const filter = this.activeFilter();
    return this.notifications().filter((n) => {
      if (n.type === 'FollowRequest') return false;
      if (filter === 'all') return true;
      if (filter === 'comments') return COMMENT_TYPES.has(n.type);
      return LIKE_TYPES.has(n.type);
    });
  }

  get thisMonthNotifications(): AppNotification[] {
    return this.visibleNotifications.filter((n) => this.isThisMonth(n.createdDate));
  }

  get earlierNotifications(): AppNotification[] {
    return this.visibleNotifications.filter((n) => !this.isThisMonth(n.createdDate));
  }

  private isThisMonth(iso: string): boolean {
    const date = new Date(iso);
    const now = new Date();
    return date.getFullYear() === now.getFullYear() && date.getMonth() === now.getMonth();
  }

  loadMore(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    this.loadingMore.set(true);

    this.notificationService.getNotifications(this.notifications().length, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.notifications.update((current) => [...current, ...items]);
        this.hasMore.set(hasMore);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false)
    });
  }

  openFollowRequests(): void {
    this.close();
    this.router.navigateByUrl('/accounts/follow-requests');
  }

  /** Routes to the right detail depending on the notification's type, and marks it read along the way. */
  open(n: AppNotification): void {
    if (!n.isRead) this.notificationService.markRead(n.id).subscribe();

    switch (n.type) {
      case 'Follow':
      case 'FollowAccepted':
        this.close();
        this.router.navigate(['/profile', n.actors[0]?.username]);
        break;
      case 'LikePost':
      case 'CommentPost':
        if (n.postId) {
          this.close();
          this.postDetailService.open(n.postId);
        }
        break;
      case 'LikeStory':
      case 'CommentStory':
        if (n.storyId) {
          this.close();
          this.storyViewerService.openStory(n.storyId);
        }
        break;
      default:
        break;
    }
  }

  isFollowPending(userId: string): boolean {
    return this.pendingFollowActors().has(userId);
  }

  /** Follow (or re-follow)/unfollow directly from a Follow-type row, without navigating away. */
  toggleFollowActor(n: AppNotification, event: Event): void {
    event.stopPropagation();
    const actor = n.actors[0];
    if (!actor || this.isFollowPending(actor.userId)) return;

    this.pendingFollowActors.update((set) => new Set(set).add(actor.userId));
    const request$ = n.isFollowingActor ? this.profileService.unfollow(actor.username) : this.profileService.follow(actor.username);

    request$.subscribe({
      next: (result) => {
        this.notifications.update((list) => list.map((item) => (item.id === n.id ? { ...item, isFollowingActor: result.isFollowing } : item)));
        this.clearFollowPending(actor.userId);
      },
      error: () => this.clearFollowPending(actor.userId)
    });
  }

  private clearFollowPending(userId: string): void {
    this.pendingFollowActors.update((set) => {
      const next = new Set(set);
      next.delete(userId);
      return next;
    });
  }

  /** "N phút/giờ/ngày/tuần" — same style used across the app's post/story timestamps. */
  relativeTime(iso: string): string {
    const mins = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 60000));
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    if (days < 7) return isVi ? `${days} ngày` : `${days}d`;
    const weeks = Math.round(days / 7);
    return isVi ? `${weeks} tuần` : `${weeks}w`;
  }
}
