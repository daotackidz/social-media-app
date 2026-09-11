import { NgClass } from '@angular/common';
import { Component, Input, signal } from '@angular/core';

import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { SuggestedUser } from '../../models/feed.models';

@Component({
  selector: 'app-suggestions-inline',
  standalone: true,
  imports: [NgClass, TranslatePipe],
  templateUrl: './suggestions-inline.component.html',
  styleUrl: './suggestions-inline.component.scss'
})
export class SuggestionsInlineComponent {
  @Input() suggestions: SuggestedUser[] = [];

  readonly dismissedIds = signal<Set<string>>(new Set());
  readonly followedIds = signal<Set<string>>(new Set());

  get visible(): SuggestedUser[] {
    return this.suggestions.filter((u) => !this.dismissedIds().has(u.id));
  }

  dismiss(id: string): void {
    this.dismissedIds.update((set) => new Set(set).add(id));
  }

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
}
