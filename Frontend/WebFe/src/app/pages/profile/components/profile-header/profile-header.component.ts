import { Component, Input, OnChanges, SimpleChanges, inject, signal } from '@angular/core';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { UserProfileSummary } from '../../models/profile.models';
import { ProfileService } from '../../services/profile.service';

@Component({
  selector: 'app-profile-header',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './profile-header.component.html',
  styleUrl: './profile-header.component.scss'
})
export class ProfileHeaderComponent implements OnChanges {
  @Input({ required: true }) profile!: UserProfileSummary;

  private readonly languageService = inject(LanguageService);
  private readonly profileService = inject(ProfileService);

  readonly isFollowing = signal(false);
  readonly followersCount = signal(0);
  readonly followPending = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profile']) {
      this.isFollowing.set(this.profile.isFollowing);
      this.followersCount.set(this.profile.stats.followersCount);
    }
  }

  toggleFollow(): void {
    if (this.followPending()) return;

    this.followPending.set(true);
    const request$ = this.isFollowing()
      ? this.profileService.unfollow(this.profile.username)
      : this.profileService.follow(this.profile.username);

    request$.subscribe({
      next: (result) => {
        this.isFollowing.set(result.isFollowing);
        this.followersCount.set(result.followersCount);
        this.followPending.set(false);
      },
      error: () => this.followPending.set(false)
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
