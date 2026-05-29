import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface APIResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  statusCode?: number;
}

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

export interface LoginResponse {
  email?: string;
  accessToken?: string;
  expiresIn?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly authBaseUrl = '/api/auth';
  private readonly accountBaseUrl = '/api/account';

  register(payload: RegisterRequest): Observable<APIResponse<RegisterResponse>> {
    return this.http.post<APIResponse<RegisterResponse>>(`${this.authBaseUrl}/register`, payload);
  }

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.accountBaseUrl}/login`, payload);
  }
}
