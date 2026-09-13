import { NgClass } from '@angular/common';
import { Component, Input, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { AuthService } from '../../../../services/auth.service';
import { ProfileService } from '../../../profile/services/profile.service';
import { CurrentUserSummary, SuggestedUser } from '../../models/feed.models';

interface FollowState {
  following: boolean;
  requested: boolean;
}

@Component({
  selector: 'app-suggestions-panel',
  standalone: true,
  imports: [NgClass, RouterLink, TranslatePipe],
  templateUrl: './suggestions-panel.component.html',
  styleUrl: './suggestions-panel.component.scss'
})
export class SuggestionsPanelComponent {
  @Input() currentUser: CurrentUserSummary = { username: '', fullName: '', avatarColor: '#405de6' };
  @Input() suggestions: SuggestedUser[] = [];

  private readonly authService = inject(AuthService);
  private readonly profileService = inject(ProfileService);
  private readonly router = inject(Router);
  readonly languageService = inject(LanguageService);

  private readonly followState = signal<Record<string, FollowState>>({});
  private readonly pendingIds = signal<Set<string>>(new Set());

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

  switchAccount(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
