import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { AuthService } from '../../services/auth.service';
import { AppInputComponent } from '../../shared/components';

interface ResetPasswordNavState {
  email?: string;
  otpCode?: string;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AppInputComponent],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  private email = '';
  private otpCode = '';

  isLoading = signal(false);
  error = signal('');

  form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  });

  constructor() {
    const navState = (this.router.getCurrentNavigation()?.extras?.state
      ?? history.state) as ResetPasswordNavState;

    this.email = navState?.email ?? '';
    this.otpCode = navState?.otpCode ?? '';

    if (!this.email || !this.otpCode) {
      this.router.navigateByUrl('/forgot-password');
    }
  }

  get newPassword() {
    return this.form.get('newPassword') as FormControl;
  }

  get confirmPassword() {
    return this.form.get('confirmPassword') as FormControl;
  }

  get isSubmitDisabled() {
    return this.isLoading() || this.form.invalid;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { newPassword, confirmPassword } = this.form.value;

    if (newPassword !== confirmPassword) {
      this.error.set('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.error.set('');
    this.isLoading.set(true);

    this.authService.resetPassword({
      email: this.email,
      otpCode: this.otpCode,
      newPassword: newPassword ?? '',
      confirmPassword: confirmPassword ?? ''
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigateByUrl('/login');
      },
      error: (err) => {
        this.isLoading.set(false);
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'Đặt lại mật khẩu thất bại. Vui lòng thử lại.');
      }
    });
  }
}
