import { NgTemplateOutlet } from '@angular/common';
import { Component, EventEmitter, OnDestroy, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { SearchHistoryItem, SearchService, SearchUser } from '../../../../services/search.service';

/** Debounce before firing a live search request while the user types. */
const SEARCH_DEBOUNCE_MS = 300;

@Component({
  selector: 'app-search-popup',
  standalone: true,
  imports: [FormsModule, NgTemplateOutlet, TranslatePipe],
  templateUrl: './search-popup.component.html',
  styleUrl: './search-popup.component.scss'
})
export class SearchPopupComponent implements OnInit, OnDestroy {
  /** Emitted once a user is picked (from history or live results) so the parent can close the popup. */
  @Output() picked = new EventEmitter<void>();

  private readonly searchService = inject(SearchService);
  private readonly router = inject(Router);

  readonly query = signal('');
  readonly history = signal<SearchHistoryItem[]>([]);
  readonly results = signal<SearchUser[]>([]);
  readonly loading = signal(false);

  private debounceHandle?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.searchService.getHistory().subscribe({
      next: (res) => this.history.set(res.data ?? []),
      error: () => this.history.set([])
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.debounceHandle);
  }

  onQueryInput(value: string): void {
    this.query.set(value);
    clearTimeout(this.debounceHandle);

    const trimmed = value.trim();
    if (!trimmed) {
      this.results.set([]);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.debounceHandle = setTimeout(() => this.runSearch(trimmed), SEARCH_DEBOUNCE_MS);
  }

  clearQuery(): void {
    this.query.set('');
    this.results.set([]);
    this.loading.set(false);
    clearTimeout(this.debounceHandle);
  }

  selectUser(user: SearchUser): void {
    this.searchService.addHistory(user.userId).subscribe();
    this.picked.emit();
    this.router.navigate(['/profile', user.username]);
  }

  removeHistory(item: SearchHistoryItem, event: MouseEvent): void {
    event.stopPropagation();
    this.searchService.deleteHistory(item.id).subscribe({
      next: () => this.history.update((list) => list.filter((h) => h.id !== item.id))
    });
  }

  clearAllHistory(): void {
    this.searchService.clearHistory().subscribe({
      next: () => this.history.set([])
    });
  }

  private runSearch(query: string): void {
    this.searchService.searchUsers(query).subscribe({
      next: (res) => {
        this.results.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.results.set([]);
        this.loading.set(false);
      }
    });
  }
}
