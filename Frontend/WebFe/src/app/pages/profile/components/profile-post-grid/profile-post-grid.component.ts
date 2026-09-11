import { Component, EventEmitter, Input, Output, inject } from '@angular/core';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
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
  @Output() readonly tabChange = new EventEmitter<ProfileTab>();

  private readonly languageService = inject(LanguageService);

  readonly tabs: TabDef[] = [
    { id: 'posts', labelKey: 'profile.tabs.posts' },
    { id: 'reels', labelKey: 'profile.tabs.reels' },
    { id: 'tagged', labelKey: 'profile.tabs.tagged' }
  ];

  selectTab(tab: ProfileTab): void {
    if (tab === this.activeTab) return;
    this.tabChange.emit(tab);
  }

  formatCount(value: number): string {
    if (value >= 1_000_000) return `${Math.round(value / 100_000) / 10}M`;
    if (value >= 1_000) return `${Math.round(value / 100) / 10}K`;
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return value.toLocaleString(locale);
  }
}
