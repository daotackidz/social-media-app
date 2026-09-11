import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { AuthService, RegisterRequest } from '../../services/auth.service';
import { AppInputComponent, AppSelectComponent, SelectOption } from '../../shared/components';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AppInputComponent,
    AppSelectComponent
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  isLoading = signal(false);
  error = signal('');
  success = signal('');

  dayOptions: SelectOption[] = Array.from({ length: 31 }, (_, i) => ({
    value: i + 1,
    label: String(i + 1)
  }));

  monthOptions: SelectOption[] = [
    { value: 1, label: 'Tháng 1' },
    { value: 2, label: 'Tháng 2' },
    { value: 3, label: 'Tháng 3' },
    { value: 4, label: 'Tháng 4' },
    { value: 5, label: 'Tháng 5' },
    { value: 6, label: 'Tháng 6' },
    { value: 7, label: 'Tháng 7' },
    { value: 8, label: 'Tháng 8' },
    { value: 9, label: 'Tháng 9' },
    { value: 10, label: 'Tháng 10' },
    { value: 11, label: 'Tháng 11' },
    { value: 12, label: 'Tháng 12' }
  ];

  yearOptions: SelectOption[] = Array.from({ length: 100 }, (_, i) => ({
    value: new Date().getFullYear() - i,
    label: String(new Date().getFullYear() - i)
  }));

  registerForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    username: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    day: [null as number | null],
    month: [null as number | null],
    year: [null as number | null]
  });

  submit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const formValue = this.registerForm.value;
    const day = formValue.day;
    const month = formValue.month;
    const year = formValue.year;

    if ((day || month || year) && !(day && month && year)) {
      this.error.set('Vui lòng chọn đầy đủ ngày, tháng và năm sinh hoặc không chọn cả ba.');
      return;
    }

    let dob: Date | null = null;
    if (day && month && year) {
      dob = new Date(Number(year), Number(month) - 1, Number(day));
    }

    const payload: RegisterRequest = {
      email: formValue.email ?? '',
      fullName: formValue.fullName ?? '',
      username: formValue.username ?? '',
      password: formValue.password ?? '',
      dateOfBirth: dob ?? ''
    };

    this.error.set('');
    this.success.set('');
    this.isLoading.set(true);

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.success.set(response.message || 'Đăng ký thành công! Vui lòng kiểm tra email để xác thực.');
        this.router.navigate(['/verify-otp'], {
          queryParams: {
            email: payload.email,
            expiresIn: response.data?.otpExpiresInSeconds
          }
        });
      },
      error: (err) => {
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'Đăng ký thất bại. Vui lòng kiểm tra lại thông tin và thử lại.');
      },
      complete: () => this.isLoading.set(false)
    });
  }

  get email() {
    return this.registerForm.get('email') as FormControl;
  }

  get fullName() {
    return this.registerForm.get('fullName') as FormControl;
  }

  get username() {
    return this.registerForm.get('username') as FormControl;
  }

  get password() {
    return this.registerForm.get('password') as FormControl;
  }

  get day() {
    return this.registerForm.get('day') as FormControl;
  }

  get month() {
    return this.registerForm.get('month') as FormControl;
  }

  get year() {
    return this.registerForm.get('year') as FormControl;
  }

  dobPartial() {
    const v = this.registerForm.value;
    return (!!v.day || !!v.month || !!v.year) && !(v.day && v.month && v.year);
  }
}

