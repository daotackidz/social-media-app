import { Component, computed, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { FooterComponent } from './shared/layout';
import { AppSplashComponent } from './core/splash/app-splash.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FooterComponent, AppSplashComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  protected readonly title = signal('WebFe');

  // The home feed reproduces its own compact footer (per the Figma design),
  // so the global site footer is hidden there to avoid showing it twice.
  private readonly currentUrl = signal(window.location.pathname);
  protected readonly showGlobalFooter = computed(() => !this.currentUrl().startsWith('/home'));

  constructor(router: Router) {
    router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.currentUrl.set(event.urlAfterRedirects));
  }
}
