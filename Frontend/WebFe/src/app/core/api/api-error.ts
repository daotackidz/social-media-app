import { HttpErrorResponse } from '@angular/common/http';

import { ApiResponse } from './api-response.model';

/**
 * Safely pulls the ApiResponse<T> envelope out of a failed HttpClient
 * request. The backend always returns this shape on 4xx/5xx (see
 * BaseApiController, Program.cs's InvalidModelStateResponseFactory, and
 * ExceptionMiddleware), so every component reads `.message` / `.errorCode`
 * the same way instead of each guessing at `err.error`'s shape.
 */
export function apiErrorOf(err: unknown): ApiResponse<never> | null {
  if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
    return err.error as ApiResponse<never>;
  }
  return null;
}
