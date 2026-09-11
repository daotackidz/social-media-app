import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AppLang } from './translations';
import { LanguageService } from './language.service';

/**
 * Backs the `/vn` and `/en` routes: sets the app language from the route's
 * static `data.lang`, then redirects back to wherever the user was (or '/'
 * on first load). Never actually activates a component.
 */
export const languageSwitchGuard: CanActivateFn = (route) => {
  const languageService = inject(LanguageService);
  const router = inject(Router);

  const lang = route.data['lang'] as AppLang;
  languageService.setLanguage(lang);

  return router.parseUrl(languageService.returnUrl());
};
