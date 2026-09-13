import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Lets any component (e.g. a profile page's "Nhắn tin" button) ask the globally-mounted
 * ChatWidgetComponent (see app.html) to open its mini chat popup for a given user — without
 * either one needing a direct reference to the other.
 */
@Injectable({ providedIn: 'root' })
export class ChatWidgetService {
  private readonly openRequests = new Subject<string>();
  readonly openRequests$ = this.openRequests.asObservable();

  /** Opens (or creates) the widget's mini chat thread with this username. */
  openConversationWith(username: string): void {
    this.openRequests.next(username);
  }
}
