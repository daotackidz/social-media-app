import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService, LoginRequest } from '../../services/auth.service';
import { AppInputComponent } from '../../shared/components';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    AppInputComponent
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  isLoading = signal(false);
  error = signal('');
  success = signal('');

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const payload: LoginRequest = this.loginForm.value as LoginRequest;
    this.error.set('');
    this.success.set('');
    this.isLoading.set(true);

    this.authService.login(payload).subscribe({
      next: () => {
        this.success.set('Đăng nhập thành công, đang chuyển hướng...');
      },
      error: () => {
        this.error.set('Đăng nhập thất bại. Kiểm tra email và mật khẩu.');
      },
      complete: () => this.isLoading.set(false)
    });
  }

  get email() {
    return this.loginForm.get('email') as FormControl;
  }

  get password() {
    return this.loginForm.get('password') as FormControl;
  }
}
