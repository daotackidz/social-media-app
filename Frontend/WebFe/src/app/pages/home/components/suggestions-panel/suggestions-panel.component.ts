import { NgClass } from '@angular/common';
import { Component, Input, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { AuthService } from '../../../../services/auth.service';
import { CurrentUserSummary, SuggestedUser } from '../../models/feed.models';

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
  private readonly router = inject(Router);
  readonly languageService = inject(LanguageService);

  readonly followedIds = signal<Set<string>>(new Set());

  toggleFollow(id: string): void {
    this.followedIds.update((set) => {
      const next = new Set(set);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  isFollowed(id: string): boolean {
    return this.followedIds().has(id);
  }

  switchAccount(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
