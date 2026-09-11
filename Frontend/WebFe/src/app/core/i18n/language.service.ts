import { Injectable, inject, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

import { AppLang, DEFAULT_LANG, LANGUAGE_STORAGE_KEY, TRANSLATIONS } from './translations';

/**
 * Minimal, dependency-free i18n service built entirely on Angular's own
 * primitives (Injectable + signal) instead of a third-party i18n library.
 * The active language is persisted to localStorage and read back on boot;
 * `LanguageSwitchGuard` (routes `/vn` and `/en`) is the only other place
 * that changes it.
 */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly lang = signal<AppLang>(this.readInitialLang());

  /** Last navigated URL that wasn't itself a /vn or /en switch link, so the
   *  switch guard can bounce the user back to where they were. */
  readonly returnUrl = signal<string>('/');

  constructor() {
    inject(Router)
      .events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.setReturnUrl(event.urlAfterRedirects));
  }

  private readInitialLang(): AppLang {
    try {
      const saved = localStorage.getItem(LANGUAGE_STORAGE_KEY);
      if (saved === 'vi' || saved === 'en') {
        return saved;
      }
    } catch {
      // localStorage unavailable (private mode, SSR, etc.) — fall back below.
    }
    return DEFAULT_LANG;
  }

  setLanguage(lang: AppLang): void {
    this.lang.set(lang);
    try {
      localStorage.setItem(LANGUAGE_STORAGE_KEY, lang);
    } catch {
      // Ignore write failures; the in-memory signal still reflects the choice.
    }
  }

  setReturnUrl(url: string): void {
    if (url && url !== '/vn' && url !== '/en') {
      this.returnUrl.set(url);
    }
  }

  /** Translate `key` for the current language, replacing `{token}` placeholders
   *  with values from `params`. Falls back to the key itself if missing. */
  t(key: string, params?: Record<string, string | number>): string {
    const dict = TRANSLATIONS[this.lang()];
    let value = dict[key] ?? key;
    if (params) {
      for (const [token, replacement] of Object.entries(params)) {
        value = value.replace(`{${token}}`, String(replacement));
      }
    }
    return value;
  }
}
