import { Component, OnDestroy, inject, signal } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';
import { Subscription } from 'rxjs';

/** A near-instant navigation (cached lazy chunk, simple guard) should never
 *  flash the overlay — only show it once a transition has actually taken a
 *  moment. */
const SHOW_DELAY_MS = 150;
/** Once shown, stay up at least this long so it can't flicker on and off for
 *  a navigation that finishes just after the delay above. */
const MIN_VISIBLE_MS = 400;

/**
 * Full-page splash shown while the router is transitioning between pages —
 * covers lazy-chunk downloads and route guards/resolvers so a slow
 * navigation doesn't leave the user staring at a stale or blank page.
 */
@Component({
  selector: 'app-splash',
  standalone: true,
  templateUrl: './app-splash.component.html',
  styleUrl: './app-splash.component.scss'
})
export class AppSplashComponent implements OnDestroy {
  private readonly router = inject(Router);

  readonly visible = signal(false);

  private showTimer?: ReturnType<typeof setTimeout>;
  private hideTimer?: ReturnType<typeof setTimeout>;
  private shownAt = 0;
  private readonly sub: Subscription;

  constructor() {
    this.sub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.scheduleShow();
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.scheduleHide();
      }
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    clearTimeout(this.showTimer);
    clearTimeout(this.hideTimer);
  }

  private scheduleShow(): void {
    clearTimeout(this.hideTimer);
    clearTimeout(this.showTimer);
    this.showTimer = setTimeout(() => {
      this.visible.set(true);
      this.shownAt = Date.now();
    }, SHOW_DELAY_MS);
  }

  private scheduleHide(): void {
    clearTimeout(this.showTimer);
    if (!this.visible()) return;

    const remaining = Math.max(0, MIN_VISIBLE_MS - (Date.now() - this.shownAt));
    this.hideTimer = setTimeout(() => this.visible.set(false), remaining);
  }
}
