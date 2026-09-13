import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { LanguageService } from '../../core/i18n/language.service';
import { StoryDetail } from '../../core/story-viewer/story.models';
import { StoryViewerService } from '../../core/story-viewer/story-viewer.service';
import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';

/** "Kho lưu trữ" — every story the signed-in user has ever posted (including expired ones), visible only to them. */
@Component({
  selector: 'app-story-archive',
  standalone: true,
  imports: [RouterLink, TranslatePipe, SidebarNavComponent],
  templateUrl: './story-archive.component.html'
})
export class StoryArchiveComponent {
  private readonly authService = inject(AuthService);
  private readonly storyViewerService = inject(StoryViewerService);
  private readonly languageService = inject(LanguageService);

  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly stories = signal<StoryDetail[]>([]);
  readonly hasMore = signal(false);
  readonly loadingMore = signal(false);

  readonly sidebarUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });
  readonly sidebarAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  constructor() {
    this.storyViewerService.getArchive(0, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.stories.set(items);
        this.hasMore.set(hasMore);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set(true);
        this.loading.set(false);
      }
    });
  }

  loadMore(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    this.loadingMore.set(true);

    this.storyViewerService.getArchive(this.stories().length, 30).subscribe({
      next: ({ items, hasMore }) => {
        this.stories.update((current) => [...current, ...items]);
        this.hasMore.set(hasMore);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false)
    });
  }

  open(index: number): void {
    this.storyViewerService.openFlat(this.stories(), index);
  }

  /** "31 Tháng 8" style date badge, like the reference screenshot. */
  dayLabel(iso: string): string {
    return new Date(iso).getDate().toString();
  }

  monthYearLabel(iso: string): string {
    const date = new Date(iso);
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    const month = new Intl.DateTimeFormat(locale, { month: 'long' }).format(date);
    return `${month} ${date.getFullYear()}`;
  }
}
