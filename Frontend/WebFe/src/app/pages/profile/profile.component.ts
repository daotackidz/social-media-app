import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

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

      this.profileService.getProfile(username).subscribe((p) => this.profile.set(p));
      this.profileService.getHighlights(username).subscribe((h) => this.highlights.set(h));
      this.loadPosts('posts');
    });
  }

  onTabChange(tab: ProfileTab): void {
    this.activeTab.set(tab);
    this.loadPosts(tab);
  }

  private loadPosts(tab: ProfileTab): void {
    this.profileService.getPosts(this.viewedUsername, tab).subscribe((p) => this.posts.set(p));
  }
}
