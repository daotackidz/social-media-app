import { Component, ElementRef, OnDestroy, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';

import { SearchService, SearchUser } from '../../../../services/search.service';
import { MessagesService } from '../../services/messages.service';

const SEARCH_DEBOUNCE_MS = 300;

/** "Tin nhắn mới" — search for one person, then "Chat" gets/creates the 1-1 thread and hands its id back to the caller. */
@Component({
  selector: 'app-new-message-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './new-message-modal.component.html',
  styleUrl: './new-message-modal.component.scss'
})
export class NewMessageModalComponent implements OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<NewMessageModalComponent, string | undefined>);
  private readonly searchService = inject(SearchService);
  private readonly messagesService = inject(MessagesService);

  readonly query = signal('');
  readonly results = signal<SearchUser[]>([]);
  readonly loading = signal(false);
  readonly selected = signal<SearchUser | null>(null);
  readonly starting = signal(false);

  private readonly queryInput = viewChild<ElementRef<HTMLInputElement>>('queryInput');
  private debounceHandle?: ReturnType<typeof setTimeout>;

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

  private runSearch(query: string): void {
    this.searchService.searchUsers(query).subscribe({
      next: (res) => {
        this.results.set((res.data ?? []).filter((u) => u.userId !== this.selected()?.userId));
        this.loading.set(false);
      },
      error: () => {
        this.results.set([]);
        this.loading.set(false);
      }
    });
  }

  select(user: SearchUser): void {
    this.selected.set(user);
    this.query.set('');
    this.results.set([]);
    this.queryInput()?.nativeElement.focus();
  }

  clearSelected(): void {
    this.selected.set(null);
  }

  close(): void {
    this.dialogRef.close(undefined);
  }

  startChat(): void {
    const user = this.selected();
    if (!user || this.starting()) return;

    this.starting.set(true);
    this.messagesService.startConversation(user.username).subscribe({
      next: (conversation) => this.dialogRef.close(conversation.id),
      error: () => this.starting.set(false)
    });
  }
}
