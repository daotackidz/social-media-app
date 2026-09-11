import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from '../../services/auth.service';

/**
 * Attaches `Authorization: Bearer <token>` to every request to our own API.
 * Without this, HttpClient never sends the JWT AuthService stores after
 * login, so every [Authorize] endpoint (profile, follow, ...) 401s even for
 * a signed-in user.
 */
export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  const token = inject(AuthService).getAccessToken();
  if (!token) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
