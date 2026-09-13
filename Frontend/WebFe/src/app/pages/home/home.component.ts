import { Component, ElementRef, OnDestroy, ViewChild, afterNextRender, computed, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { CreatePostService } from '../../core/create-post/create-post.service';
import { CreateStoryService } from '../../core/create-story/create-story.service';
import { StoryViewerService } from '../../core/story-viewer/story-viewer.service';
import { SidebarNavComponent } from './components/sidebar-nav/sidebar-nav.component';
import { StoriesBarComponent } from './components/stories-bar/stories-bar.component';
import { FeedPostComponent } from './components/feed-post/feed-post.component';
import { SuggestionsPanelComponent } from './components/suggestions-panel/suggestions-panel.component';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { FeedPost, StoryItem, SuggestedUser } from './models/feed.models';
import { FeedService } from './services/feed.service';

const POSTS_PAGE_SIZE = 10;

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    SidebarNavComponent,
    StoriesBarComponent,
    FeedPostComponent,
    SuggestionsPanelComponent,
    TranslatePipe
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnDestroy {
  private readonly feedService = inject(FeedService);
  private readonly authService = inject(AuthService);
  private readonly createPostService = inject(CreatePostService);
  private readonly createStoryService = inject(CreateStoryService);
  private readonly storyViewerService = inject(StoryViewerService);

  readonly stories = signal<StoryItem[]>([]);
  readonly posts = signal<FeedPost[]>([]);
  readonly panelSuggestions = signal<SuggestedUser[]>([]);

  /** Infinite-scroll state for the post list — the stories tray is a single fixed-size fetch, not paged. */
  private postsSkip = 0;
  readonly postsLoading = signal(false);
  readonly postsHasMore = signal(true);

  /** "You're all caught up" is shown once loading stops and there is nothing left to fetch — including when there were no posts at all to begin with. */
  readonly showCaughtUp = computed(() => !this.postsHasMore() && !this.postsLoading());

  /** Days spanned by the loaded feed — matches the "...from the past N days" caught-up copy. */
  readonly caughtUpDays = computed(() => {
    const items = this.posts();
    if (items.length === 0) return 1;
    const oldest = items[items.length - 1].createdDate;
    const days = Math.round((Date.now() - new Date(oldest).getTime()) / (24 * 60 * 60 * 1000));
    return Math.max(1, days);
  });

  @ViewChild('scrollSentinel') private scrollSentinel?: ElementRef<HTMLElement>;
  private scrollObserver?: IntersectionObserver;
  private readonly postCreatedSub: Subscription;
  private readonly storyCreatedSub: Subscription;
  private readonly storyViewedSub: Subscription;

  readonly currentUser = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    const username = this.authService.currentUsername() ?? fallback;
    return {
      username,
      fullName: this.authService.currentFullName() ?? '',
      avatarUrl: this.authService.currentAvatarUrl() ?? undefined,
      avatarColor: '#405de6'
    };
  });

  constructor() {
    this.authService.loadCurrentUserProfile();
    this.feedService.getStories().subscribe((page) => this.stories.set(page.items));
    this.loadMorePosts();
    this.feedService.getPanelSuggestions().subscribe((suggestions) => this.panelSuggestions.set(suggestions));

    // The sentinel only exists once the template renders — wire the observer up after the first paint.
    afterNextRender(() => this.observeSentinel());

    // A post created from the "new post" modal (open from anywhere via the
    // sidebar) should show up here right away, so reload the feed from the top.
    this.postCreatedSub = this.createPostService.postCreated$.subscribe(() => this.refreshFeed());

    // A story created just now should appear in the tray immediately too.
    this.storyCreatedSub = this.createStoryService.storyCreated$.subscribe(() => this.refreshStories());

    // Grays out a ring the moment its owner's story is marked viewed in the
    // open viewer, without waiting on a full tray refetch.
    this.storyViewedSub = this.storyViewerService.storyViewed$.subscribe((ownerUserId) => {
      this.stories.update((current) => current.map((s) => (s.userId === ownerUserId ? { ...s, viewed: true } : s)));
    });
  }

  ngOnDestroy(): void {
    this.scrollObserver?.disconnect();
    this.postCreatedSub.unsubscribe();
    this.storyCreatedSub.unsubscribe();
    this.storyViewedSub.unsubscribe();
  }

  private refreshStories(): void {
    this.feedService.getStories().subscribe((page) => this.stories.set(page.items));
  }

  /** Resets pagination and reloads the feed from the top — used after creating a post. */
  private refreshFeed(): void {
    this.postsSkip = 0;
    this.postsHasMore.set(true);
    this.posts.set([]);
    this.loadMorePosts();
  }

  loadMorePosts(): void {
    if (this.postsLoading() || !this.postsHasMore()) return;

    this.postsLoading.set(true);
    this.feedService.getPosts(this.postsSkip, POSTS_PAGE_SIZE).subscribe({
      next: (page) => {
        this.posts.update((current) => [...current, ...page.items]);
        this.postsSkip += page.items.length;
        this.postsHasMore.set(page.hasMore);
        this.postsLoading.set(false);
      },
      error: () => this.postsLoading.set(false)
    });
  }

  /** Fires loadMorePosts() once the sentinel below the last post scrolls into view. */
  private observeSentinel(): void {
    const target = this.scrollSentinel?.nativeElement;
    if (!target) return;

    this.scrollObserver = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) this.loadMorePosts();
      },
      { rootMargin: '600px 0px' }
    );
    this.scrollObserver.observe(target);
  }
}
