import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

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

  isLoading = signal(false);
  error = signal('');
  success = signal('');

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

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
      next: () => {
        this.success.set('Đăng nhập thành công, đang chuyển hướng...');
      },
      error: () => {
        this.error.set('Đăng nhập thất bại. Kiểm tra email và mật khẩu.');
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
