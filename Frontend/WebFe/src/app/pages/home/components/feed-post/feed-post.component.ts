import { Component, Input, inject, signal } from '@angular/core';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { FeedPost } from '../../models/feed.models';

@Component({
  selector: 'app-feed-post',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './feed-post.component.html',
  styleUrl: './feed-post.component.scss'
})
export class FeedPostComponent {
  @Input({ required: true }) post!: FeedPost;

  private readonly languageService = inject(LanguageService);

  readonly liked = signal(false);
  readonly saved = signal(false);

  toggleLike(): void {
    this.liked.update((v) => !v);
  }

  toggleSave(): void {
    this.saved.update((v) => !v);
  }

  get relativeTime(): string {
    const mins = this.post.postedAgoMinutes;
    const isVi = this.languageService.lang() === 'vi';
    if (mins < 60) return isVi ? `${mins} phút` : `${mins}m`;
    const hours = Math.round(mins / 60);
    if (hours < 24) return isVi ? `${hours} giờ` : `${hours}h`;
    const days = Math.round(hours / 24);
    return isVi ? `${days} ngày` : `${days}d`;
  }

  get likesDisplay(): string {
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return (this.post.likes + (this.liked() ? 1 : 0)).toLocaleString(locale);
  }
}
