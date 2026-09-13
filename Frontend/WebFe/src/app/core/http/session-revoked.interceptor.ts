import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { ApiResponse } from '../api';
import { AuthService } from '../../services/auth.service';

/**
 * A 401 with errorCode "SESSION_REVOKED" means the backend's OnTokenValidated
 * check (Program.cs) rejected this token because a newer login on the same
 * client type overwrote its session — i.e. this browser was signed out by a
 * login elsewhere. Every open tab hits this on its next request, so redirect
 * to /login with a flag it reads to show a plain inline message, instead of
 * silently failing whatever the page was doing.
 */
export const sessionRevokedInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        const body = err.error as ApiResponse<unknown> | undefined;
        if (body?.errorCode === 'SESSION_REVOKED') {
          authService.logout();
          router.navigate(['/login'], { queryParams: { sessionRevoked: 1 } });
        }
      }
      return throwError(() => err);
    })
  );
};
