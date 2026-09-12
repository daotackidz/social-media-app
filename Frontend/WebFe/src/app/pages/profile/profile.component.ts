import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { FooterComponent } from '../../shared/layout/footer/footer.component';
import { AuthService } from '../../services/auth.service';
import { ProfileHeaderComponent } from './components/profile-header/profile-header.component';
import { ProfileHighlightsComponent } from './components/profile-highlights/profile-highlights.component';
import { ProfilePostGridComponent } from './components/profile-post-grid/profile-post-grid.component';
import { ProfileHighlight, ProfilePost, ProfileTab, UserProfileSummary } from './models/profile.models';
import { ProfileService } from './services/profile.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    TranslatePipe,
    RouterLink,
    SidebarNavComponent,
    FooterComponent,
    ProfileHeaderComponent,
    ProfileHighlightsComponent,
    ProfilePostGridComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly profileService = inject(ProfileService);

  readonly profile = signal<UserProfileSummary | null>(null);
  readonly highlights = signal<ProfileHighlight[]>([]);
  readonly posts = signal<ProfilePost[]>([]);
  readonly activeTab = signal<ProfileTab>('posts');
  /** Only fetched/shown when viewing one's own profile — see the banner in profile.component.html. */
  readonly pendingRequestsCount = signal(0);

  /** Covers the profile header fetch — the part everything else on the page depends on. */
  readonly loading = signal(true);
  readonly loadError = signal(false);
  /** Covers just the post grid, so switching tabs doesn't block the whole page. */
  readonly postsLoading = signal(false);

  /** For the sidebar's own avatar/link — falls back to the email prefix for a
   *  session logged in before the backend started returning a real username. */
  readonly currentUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });

  /** Sidebar always shows the signed-in user's own avatar, regardless of whose profile is open. */
  readonly currentAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  private viewedUsername = '';

  constructor() {
    this.authService.loadCurrentUserProfile();

    this.route.paramMap.subscribe((params) => {
      const username = params.get('username') || this.currentUsername();
      this.viewedUsername = username;
      this.activeTab.set('posts');
      this.loading.set(true);
      this.loadError.set(false);
      this.profile.set(null);

      this.profileService.getProfile(username).subscribe({
        next: (p) => {
          this.profile.set(p);
          this.loading.set(false);
          this.pendingRequestsCount.set(0);
          if (p.isCurrentUser) {
            this.profileService.getFollowRequests().subscribe({
              next: (requests) => this.pendingRequestsCount.set(requests.length),
              error: () => this.pendingRequestsCount.set(0)
            });
          }
        },
        error: () => {
          this.loading.set(false);
          this.loadError.set(true);
        }
      });
      this.profileService.getHighlights(username).subscribe({
        next: (h) => this.highlights.set(h),
        error: () => this.highlights.set([])
      });
      this.loadPosts('posts');
    });
  }

  onTabChange(tab: ProfileTab): void {
    this.activeTab.set(tab);
    this.loadPosts(tab);
  }

  private loadPosts(tab: ProfileTab): void {
    this.postsLoading.set(true);
    this.profileService.getPosts(this.viewedUsername, tab).subscribe({
      next: (p) => {
        this.posts.set(p);
        this.postsLoading.set(false);
      },
      error: () => {
        this.posts.set([]);
        this.postsLoading.set(false);
      }
    });
  }
}
