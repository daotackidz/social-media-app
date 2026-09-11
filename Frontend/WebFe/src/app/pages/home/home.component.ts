import { Component, computed, inject, signal } from '@angular/core';

import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from './components/sidebar-nav/sidebar-nav.component';
import { StoriesBarComponent } from './components/stories-bar/stories-bar.component';
import { FeedPostComponent } from './components/feed-post/feed-post.component';
import { SuggestionsInlineComponent } from './components/suggestions-inline/suggestions-inline.component';
import { SuggestionsPanelComponent } from './components/suggestions-panel/suggestions-panel.component';
import { FeedPost, StoryItem, SuggestedUser } from './models/feed.models';
import { FeedService } from './services/feed.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    SidebarNavComponent,
    StoriesBarComponent,
    FeedPostComponent,
    SuggestionsInlineComponent,
    SuggestionsPanelComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private readonly feedService = inject(FeedService);
  private readonly authService = inject(AuthService);

  readonly stories = signal<StoryItem[]>([]);
  readonly posts = signal<FeedPost[]>([]);
  readonly inlineSuggestions = signal<SuggestedUser[]>([]);
  readonly panelSuggestions = signal<SuggestedUser[]>([]);

  /** Index (0-based) of the post after which the inline suggestions carousel appears. */
  private readonly inlineSuggestionsAfterPost = 1;

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
    this.feedService.getStories().subscribe((stories) => this.stories.set(stories));
    this.feedService.getPosts().subscribe((posts) => this.posts.set(posts));
    this.feedService.getInlineSuggestions().subscribe((suggestions) => this.inlineSuggestions.set(suggestions));
    this.feedService.getPanelSuggestions().subscribe((suggestions) => this.panelSuggestions.set(suggestions));
  }

  showInlineSuggestionsAfter(index: number): boolean {
    return index === this.inlineSuggestionsAfterPost;
  }
}
