import { Pipe, PipeTransform, inject } from '@angular/core';

import { LanguageService } from './language.service';

/**
 * `{{ 'home.follow' | translate }}` — impure on purpose so the text updates
 * as soon as `LanguageService.lang` changes, without every component having
 * to re-subscribe manually.
 */
@Pipe({
  name: 'translate',
  standalone: true,
  pure: false
})
export class TranslatePipe implements PipeTransform {
  private readonly languageService = inject(LanguageService);

  transform(key: string, params?: Record<string, string | number>): string {
    return this.languageService.t(key, params);
  }
}
