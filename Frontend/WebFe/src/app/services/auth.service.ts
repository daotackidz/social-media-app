import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { ApiResponse } from '../core/api';

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  username: string;
  dateOfBirth: string | Date;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterResponse {
  email: string;
  otpExpiresInSeconds: number;
}

export interface VerifyEmailRequest {
  email: string;
  otpCode: string;
}

export interface UserResponse {
  id: string;
  email: string;
  fullName?: string;
  dateOfBirth?: string;
  createdDate?: string;
  avatarUrl?: string;
}

export interface LoginResponse {
  email?: string;
  username?: string;
  accessToken?: string;
  expiresIn?: number;
}

const ACCESS_TOKEN_KEY = 'access_token';
const CURRENT_EMAIL_KEY = 'current_email';
const CURRENT_USERNAME_KEY = 'current_username';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly authBaseUrl = '/api/auth';
  private readonly accountBaseUrl = '/api/account';

  /** Email of the signed-in user, if any — read from storage on boot so it
   *  survives a page refresh; kept in sync by setSession()/logout(). */
  readonly currentEmail = signal<string | null>(this.readStorage(CURRENT_EMAIL_KEY));

  /** @username of the signed-in user (used for profile routes/links). */
  readonly currentUsername = signal<string | null>(this.readStorage(CURRENT_USERNAME_KEY));

  /** Own avatar/full name — not persisted to storage, just cached in memory for
   *  the session (sidebar, "switch account" panel, ...). Populated on demand by
   *  loadCurrentUserProfile(), since it takes an extra request the login
   *  response doesn't carry. */
  readonly currentAvatarUrl = signal<string | null>(null);
  readonly currentFullName = signal<string | null>(null);

  register(payload: RegisterRequest): Observable<ApiResponse<RegisterResponse>> {
    return this.http.post<ApiResponse<RegisterResponse>>(`${this.authBaseUrl}/register`, payload);
  }

  verifyEmail(payload: VerifyEmailRequest): Observable<ApiResponse<UserResponse>> {
    return this.http.post<ApiResponse<UserResponse>>(`${this.authBaseUrl}/verify-email`, payload);
  }

  resendOtp(email: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.authBaseUrl}/resend-otp`, { email });
  }

  login(payload: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.accountBaseUrl}/login`, payload).pipe(
      tap((response) => {
        if (response.data) {
          this.setSession(response.data, payload.email);
        }
      })
    );
  }

  setSession(response: LoginResponse, email: string): void {
    if (response.accessToken) {
      this.writeStorage(ACCESS_TOKEN_KEY, response.accessToken);
    }
    this.writeStorage(CURRENT_EMAIL_KEY, response.email ?? email);
    this.currentEmail.set(response.email ?? email);

    if (response.username) {
      this.writeStorage(CURRENT_USERNAME_KEY, response.username);
      this.currentUsername.set(response.username);
    }
  }

  logout(): void {
    try {
      localStorage.removeItem(ACCESS_TOKEN_KEY);
      localStorage.removeItem(CURRENT_EMAIL_KEY);
      localStorage.removeItem(CURRENT_USERNAME_KEY);
    } catch {
      // ignore
    }
    this.currentEmail.set(null);
    this.currentUsername.set(null);
    this.currentAvatarUrl.set(null);
    this.currentFullName.set(null);
  }

  /** Fetches the signed-in user's own avatar/full name from the profile API
   *  and caches them in currentAvatarUrl/currentFullName. Safe to call every
   *  time a page that shows them (Home, Profile, ...) loads — it's one
   *  lightweight GET, and components just read the signals it fills in. */
  loadCurrentUserProfile(): void {
    const username = this.currentUsername();
    if (!username) return;

    this.http.get<ApiResponse<{ fullName?: string; avatarUrl?: string }>>(`/api/profile/${username}`).subscribe({
      next: (res) => {
        this.currentAvatarUrl.set(res.data?.avatarUrl ?? null);
        this.currentFullName.set(res.data?.fullName ?? null);
      },
      error: () => {
        // Non-fatal — sidebar/panel just keep showing the initial fallback.
      }
    });
  }

  /** Raw JWT for the Authorization header — see core/http/auth-token.interceptor.ts. */
  getAccessToken(): string | null {
    return this.readStorage(ACCESS_TOKEN_KEY);
  }

  /** True only if a token is stored AND its JWT `exp` claim hasn't passed yet.
   *  An expired token is cleared as a side effect, so callers never have to
   *  remember to do it themselves. */
  isLoggedIn(): boolean {
    const token = this.readStorage(ACCESS_TOKEN_KEY);
    if (!token) {
      return false;
    }
    if (this.isTokenExpired(token)) {
      this.logout();
      return false;
    }
    return true;
  }

  private isTokenExpired(token: string): boolean {
    const expiresAtMs = this.getTokenExpiryMs(token);
    // No readable `exp` claim -> fail safe as "expired" rather than trusting it forever.
    return expiresAtMs === null || Date.now() >= expiresAtMs;
  }

  private getTokenExpiryMs(token: string): number | null {
    try {
      const payloadSegment = token.split('.')[1];
      const base64 = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
      const json = atob(base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '='));
      const payload = JSON.parse(json) as { exp?: number };
      return typeof payload.exp === 'number' ? payload.exp * 1000 : null;
    } catch {
      return null;
    }
  }

  private readStorage(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private writeStorage(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      // ignore write failures (private mode, storage full, etc.)
    }
  }
}
