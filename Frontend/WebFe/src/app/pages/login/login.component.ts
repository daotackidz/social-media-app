import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { apiErrorOf } from '../../core/api';
import { AuthService, LoginRequest } from '../../services/auth.service';
import { AppInputComponent } from '../../shared/components';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AppInputComponent
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isLoading = signal(false);
  error = signal('');
  success = signal('');

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor() {
    // Landed here from sessionRevokedInterceptor (core/http) — this browser was
    // signed out because the account logged in somewhere else.
    if (this.route.snapshot.queryParamMap.get('sessionRevoked')) {
      this.error.set('Tài khoản đã được đăng nhập ở một nơi khác.');
    }
  }

  get isSubmitDisabled() {
    return this.isLoading() || this.loginForm.invalid;
  }

  submit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const payload: LoginRequest = this.loginForm.value as LoginRequest;
    this.error.set('');
    this.success.set('');
    this.isLoading.set(true);

    this.authService.login(payload).pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (response) => {
        this.success.set(response.message || 'Đăng nhập thành công, đang chuyển hướng...');
        this.router.navigateByUrl('/home');
      },
      error: (err) => {
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'Đăng nhập thất bại. Kiểm tra email và mật khẩu.');
      }
    });
  }

  get email() {
    return this.loginForm.get('email') as FormControl;
  }

  get password() {
    return this.loginForm.get('password') as FormControl;
  }
}
