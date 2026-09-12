import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { AuthService } from '../../services/auth.service';
import { AppInputComponent } from '../../shared/components';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AppInputComponent],
  templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  isLoading = signal(false);
  error = signal('');

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  get email() {
    return this.form.get('email') as FormControl;
  }

  get isSubmitDisabled() {
    return this.isLoading() || this.form.invalid;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const email = this.email.value as string;
    this.error.set('');
    this.isLoading.set(true);

    this.authService.forgotPassword(email).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.router.navigate(['/forgot-password/verify'], {
          queryParams: {
            email,
            expiresIn: response.data?.otpExpiresInSeconds
          }
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'Không thể gửi mã xác nhận. Vui lòng thử lại.');
      }
    });
  }
}
