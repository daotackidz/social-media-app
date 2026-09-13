import { Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { NavigationEnd, NavigationStart, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AppSplashComponent } from './core/splash/app-splash.component';
import { ChatWidgetComponent } from './core/chat-widget/chat-widget.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AppSplashComponent, ChatWidgetComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  protected readonly title = signal('WebFe');

  private readonly dialog = inject(MatDialog);

  constructor(router: Router) {
    router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        this.scrollToTop();
      });

    // Any open MatDialog (post detail, likes list, create post, ...) is a modal overlay
    // on top of the whole app — it isn't scoped to one route, so the router never closes
    // it on its own. Without this, navigating away (sidebar links, browser back/forward)
    // left the popup floating over the new page underneath it.
    router.events
      .pipe(filter((event): event is NavigationStart => event instanceof NavigationStart))
      .subscribe(() => this.dialog.closeAll());
  }

  /**
   * Router's own withInMemoryScrolling({ scrollPositionRestoration: 'top' }) isn't enough on
   * its own: closing a dialog is what actually re-enables background scrolling (MatDialog blocks
   * it while open), and that only happens once the dialog's close *animation* finishes — a couple
   * hundred ms after navigation has already landed on the new page. When it does, it restores the
   * window to wherever the page was scrolled to when the dialog was opened, snapping the new page
   * back down. Re-asserting scroll-to-top once more after that animation window covers it (e.g.
   * clicking a profile link inside a post's like list/comments while scrolled down the feed).
   */
  private scrollToTop(): void {
    window.scrollTo({ top: 0, left: 0, behavior: 'instant' });
    setTimeout(() => window.scrollTo({ top: 0, left: 0, behavior: 'instant' }), 300);
  }
}
