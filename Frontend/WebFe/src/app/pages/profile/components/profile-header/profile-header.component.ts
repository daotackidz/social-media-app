import { Component, HostListener, Input, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { apiErrorOf } from '../../../../core/api';
import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { UserProfileSummary } from '../../models/profile.models';
import { ProfileService } from '../../services/profile.service';

@Component({
  selector: 'app-profile-header',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './profile-header.component.html',
  styleUrl: './profile-header.component.scss'
})
export class ProfileHeaderComponent implements OnChanges {
  @Input({ required: true }) profile!: UserProfileSummary;

  private readonly languageService = inject(LanguageService);
  private readonly profileService = inject(ProfileService);

  readonly isFollowing = signal(false);
  readonly isRequested = signal(false);
  readonly followersCount = signal(0);
  readonly followPending = signal(false);
  readonly followError = signal('');

  /** Full-size avatar lightbox — opened by clicking the profile photo, closed via backdrop click, the X, or Escape. */
  readonly avatarViewerOpen = signal(false);

  openAvatarViewer(): void {
    if (this.profile.avatarUrl) this.avatarViewerOpen.set(true);
  }

  closeAvatarViewer(): void {
    this.avatarViewerOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeAvatarViewer();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profile']) {
      this.isFollowing.set(this.profile.isFollowing);
      this.isRequested.set(this.profile.isRequested);
      this.followersCount.set(this.profile.stats.followersCount);
    }
  }

  /** Follow → Requested (if the account is private) or Following; clicking either of those again cancels/unfollows. */
  toggleFollow(): void {
    if (this.followPending()) return;

    this.followPending.set(true);
    this.followError.set('');
    const request$ = this.isFollowing() || this.isRequested()
      ? this.profileService.unfollow(this.profile.username)
      : this.profileService.follow(this.profile.username);

    request$.subscribe({
      next: (result) => {
        this.isFollowing.set(result.isFollowing);
        this.isRequested.set(result.isRequested);
        this.followersCount.set(result.followersCount);
        this.followPending.set(false);
      },
      error: (err) => {
        this.followPending.set(false);
        const apiError = apiErrorOf(err);
        this.followError.set(apiError?.message || this.languageService.t('profile.followError'));
      }
    });
  }

  formatCount(value: number): string {
    if (value >= 1_000_000) {
      return `${this.trimDecimal(value / 1_000_000)}M`;
    }
    if (value >= 1_000) {
      return `${this.trimDecimal(value / 1_000)}K`;
    }
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return value.toLocaleString(locale);
  }

  private trimDecimal(value: number): string {
    return value % 1 === 0 ? `${value}` : value.toFixed(1);
  }
}
