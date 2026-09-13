import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { authTokenInterceptor } from '../core/http/auth-token.interceptor';
import { sessionRevokedInterceptor } from '../core/http/session-revoked.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authTokenInterceptor, sessionRevokedInterceptor])),
    // 'top' on every navigation (not just new-entry ones) so a page never
    // opens still scrolled from whatever position the previous page was at;
    // 'enabled' restores scroll on back/forward instead, which we don't want.
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'top', anchorScrolling: 'enabled' })),
    // Required by Angular Material components (e.g. MatDialog for the
    // create-post modal) — without it they fall back with a console warning.
    provideAnimationsAsync()
  ]
};
