import { Component } from '@angular/core';

/**
 * Nominal route target for `/vn` and `/en`. `languageSwitchGuard` always
 * redirects before this ever renders — it only exists to satisfy the router's
 * "a route needs something to activate" requirement.
 */
@Component({
  selector: 'app-lang-redirect',
  standalone: true,
  template: ''
})
export class LangRedirectComponent {}
