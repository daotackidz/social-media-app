import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { PostDetailService } from '../../../../core/post-detail/post-detail.service';
import { ProfilePost, ProfileTab } from '../../models/profile.models';

interface TabDef {
  id: ProfileTab;
  labelKey: string;
}

@Component({
  selector: 'app-profile-post-grid',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './profile-post-grid.component.html',
  styleUrl: './profile-post-grid.component.scss'
})
export class ProfilePostGridComponent {
  @Input() posts: ProfilePost[] = [];
  @Input() activeTab: ProfileTab = 'posts';
  @Input() loading = false;
  /** True when this is someone else's private account we don't (yet) follow — posts are hidden, so show a locked placard instead of "no posts". */
  @Input() isPrivateLocked = false;
  @Output() readonly tabChange = new EventEmitter<ProfileTab>();

  private readonly languageService = inject(LanguageService);
  private readonly postDetailService = inject(PostDetailService);

  readonly tabs: TabDef[] = [
    { id: 'posts', labelKey: 'profile.tabs.posts' },
    { id: 'reels', labelKey: 'profile.tabs.reels' },
    { id: 'tagged', labelKey: 'profile.tabs.tagged' }
  ];

  selectTab(tab: ProfileTab): void {
    if (tab === this.activeTab) return;
    this.tabChange.emit(tab);
  }

  openPost(postId: string): void {
    this.postDetailService.open(postId);
  }

  /** Per-URL "has this actually finished downloading" flag — each cover starts hidden behind a spinner sized to its own tile and fades in on its (load) event. */
  private readonly loadedMedia = signal<Set<string>>(new Set());

  isMediaLoaded(url: string | null | undefined): boolean {
    return !url || this.loadedMedia().has(url);
  }

  /** Also used as the (error) handler — a broken image should still clear its own spinner rather than spin forever. */
  onMediaLoad(url: string): void {
    if (this.loadedMedia().has(url)) return;
    this.loadedMedia.update((set) => new Set(set).add(url));
  }

  formatCount(value: number): string {
    if (value >= 1_000_000) return `${Math.round(value / 100_000) / 10}M`;
    if (value >= 1_000) return `${Math.round(value / 100) / 10}K`;
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return value.toLocaleString(locale);
  }
}
