import { NgClass } from '@angular/common';
import { Component, ElementRef, OnDestroy, ViewChild, afterNextRender, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';
import { SuggestedUser } from '../home/models/feed.models';
import { FeedService } from '../home/services/feed.service';
import { ProfileService } from '../profile/services/profile.service';

const PAGE_SIZE = 10;

interface FollowState {
  following: boolean;
  requested: boolean;
}

/**
 * "See all suggestions" — the home sidebar card only ever shows a capped top-10
 * (FeedService.getPanelSuggestions), so this page re-fetches the same
 * mutual-connections-first ranking from FeedController but pages through it
 * 10 at a time via infinite scroll instead of stopping at one page.
 */
@Component({
  selector: 'app-suggestions',
  standalone: true,
  imports: [NgClass, RouterLink, TranslatePipe, SidebarNavComponent],
  templateUrl: './suggestions.component.html'
})
export class SuggestionsComponent implements OnDestroy {
  private readonly feedService = inject(FeedService);
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);

  readonly users = signal<SuggestedUser[]>([]);
  /** Only true for the very first fetch — the empty-list skeleton, not the "load more" footer. */
  readonly loading = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore = signal(true);

  private readonly followState = signal<Record<string, FollowState>>({});
  private readonly pendingIds = signal<Set<string>>(new Set());

  private skip = 0;
  private fetching = false;

  readonly sidebarUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });
  readonly sidebarAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  @ViewChild('scrollSentinel') private scrollSentinel?: ElementRef<HTMLElement>;
  private scrollObserver?: IntersectionObserver;

  constructor() {
    this.fetchNext();
    // The sentinel only exists once the template renders — wire the observer up after the first paint.
    afterNextRender(() => this.observeSentinel());
  }

  ngOnDestroy(): void {
    this.scrollObserver?.disconnect();
  }

  /** Follow (or send a request, for a private account) / unfollow (or cancel a pending request) — same toggle as the profile page's button. */
  toggleFollow(user: SuggestedUser): void {
    if (this.pendingIds().has(user.id)) return;

    this.pendingIds.update((set) => new Set(set).add(user.id));
    const active = this.isFollowed(user.id) || this.isRequested(user.id);
    const request$ = active ? this.profileService.unfollow(user.username) : this.profileService.follow(user.username);

    request$.subscribe({
      next: (result) => {
        this.followState.update((state) => ({
          ...state,
          [user.id]: { following: result.isFollowing, requested: result.isRequested }
        }));
        this.clearPending(user.id);
      },
      error: () => this.clearPending(user.id)
    });
  }

  isFollowed(id: string): boolean {
    return !!this.followState()[id]?.following;
  }

  isRequested(id: string): boolean {
    return !!this.followState()[id]?.requested;
  }

  isPending(id: string): boolean {
    return this.pendingIds().has(id);
  }

  private clearPending(id: string): void {
    this.pendingIds.update((set) => {
      const next = new Set(set);
      next.delete(id);
      return next;
    });
  }

  /** Fires on init and again each time the scroll sentinel comes into view. */
  private fetchNext(): void {
    if (this.fetching || !this.hasMore()) return;

    this.fetching = true;
    if (this.skip > 0) this.loadingMore.set(true);

    this.feedService.getSuggestions(this.skip, PAGE_SIZE).subscribe({
      next: (page) => {
        this.users.update((current) => [...current, ...page.items]);
        this.skip += page.items.length;
        this.hasMore.set(page.hasMore);
        this.loading.set(false);
        this.loadingMore.set(false);
        this.fetching = false;
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
        this.fetching = false;
      }
    });
  }

  private observeSentinel(): void {
    const target = this.scrollSentinel?.nativeElement;
    if (!target) return;

    this.scrollObserver = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) this.fetchNext();
      },
      { rootMargin: '600px 0px' }
    );
    this.scrollObserver.observe(target);
  }
}
