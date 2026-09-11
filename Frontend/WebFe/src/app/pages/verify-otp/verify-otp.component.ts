import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  QueryList,
  ViewChildren,
  inject,
  signal
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { AuthService } from '../../services/auth.service';

const OTP_LENGTH = 6;
const OTP_VALIDITY_SECONDS = 120; // keep in sync with Backend OtpSettings:ExpiryMinutes (2 min)
const RESEND_COOLDOWN_SECONDS = 60;

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.scss']
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  @ViewChildren('otpBox') otpBoxes!: QueryList<ElementRef<HTMLInputElement>>;

  readonly indexes = Array.from({ length: OTP_LENGTH }, (_, i) => i);

  email = '';
  digits = signal<string[]>(Array(OTP_LENGTH).fill(''));

  isVerifying = signal(false);
  isResending = signal(false);
  error = signal('');
  success = signal('');

  expirySeconds = signal(OTP_VALIDITY_SECONDS);
  resendCooldown = signal(RESEND_COOLDOWN_SECONDS);
  private timer?: ReturnType<typeof setInterval>;

  get expired(): boolean {
    return this.expirySeconds() <= 0;
  }

  get canResend(): boolean {
    return this.resendCooldown() <= 0;
  }

  get otpCode(): string {
    return this.digits().join('');
  }

  ngOnInit(): void {
    const qp = this.route.snapshot.queryParamMap;
    this.email = qp.get('email') ?? '';

    const expiresIn = Number(qp.get('expiresIn'));
    this.expirySeconds.set(Number.isFinite(expiresIn) && expiresIn > 0 ? expiresIn : OTP_VALIDITY_SECONDS);

    if (!this.email) {
      this.router.navigateByUrl('/register');
      return;
    }

    this.startTimer();
    this.focusBox(0);
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  private startTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
    }
    this.timer = setInterval(() => {
      if (this.expirySeconds() > 0) {
        this.expirySeconds.update((v) => v - 1);
      }
      if (this.resendCooldown() > 0) {
        this.resendCooldown.update((v) => v - 1);
      }
    }, 1000);
  }

  formatTime(totalSeconds: number): string {
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.replace(/\D/g, '');

    if (value.length > 1) {
      this.fillFromString(value, index);
      return;
    }

    const next = [...this.digits()];
    next[index] = value;
    this.digits.set(next);

    if (value && index < OTP_LENGTH - 1) {
      this.focusBox(index + 1);
    }
  }

  onKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits()[index] && index > 0) {
      this.focusBox(index - 1);
    }
  }

  onPaste(index: number, event: ClipboardEvent): void {
    const pasted = event.clipboardData?.getData('text') ?? '';
    if (!pasted) return;
    event.preventDefault();
    this.fillFromString(pasted, index);
  }

  private fillFromString(value: string, startIndex: number): void {
    const clean = value.replace(/\D/g, '').slice(0, OTP_LENGTH - startIndex);
    if (!clean) return;

    const next = [...this.digits()];
    let i = startIndex;
    for (const ch of clean) {
      next[i] = ch;
      i++;
    }
    this.digits.set(next);
    this.focusBox(Math.min(i, OTP_LENGTH - 1));
  }

  private focusBox(index: number): void {
    setTimeout(() => this.otpBoxes?.get(index)?.nativeElement.focus());
  }

  onFormSubmit(event: Event): void {
    event.preventDefault();
    this.submit();
  }

  submit(): void {
    this.error.set('');
    this.success.set('');

    if (this.expired) {
      this.error.set('Mã xác nhận đã hết hạn. Vui lòng gửi lại mã.');
      return;
    }

    const code = this.otpCode;
    if (code.length !== OTP_LENGTH) {
      this.error.set('Vui lòng nhập đầy đủ 6 chữ số.');
      return;
    }

    this.isVerifying.set(true);
    this.authService.verifyEmail({ email: this.email, otpCode: code }).subscribe({
      next: (response) => {
        this.isVerifying.set(false);
        this.success.set(response.message || 'Xác thực thành công! Đang chuyển tới trang đăng nhập...');
        setTimeout(() => this.router.navigateByUrl('/login'), 1200);
      },
      error: (err) => {
        this.isVerifying.set(false);
        const apiError = apiErrorOf(err);

        if (apiError?.errorCode === 'OTP_EXPIRED') {
          this.expirySeconds.set(0);
        }

        this.error.set(apiError?.message || 'Xác thực thất bại. Vui lòng thử lại.');
        this.digits.set(Array(OTP_LENGTH).fill(''));
        this.focusBox(0);
      }
    });
  }

  resend(): void {
    if (!this.canResend || this.isResending()) return;

    this.error.set('');
    this.success.set('');
    this.isResending.set(true);

    this.authService.resendOtp(this.email).subscribe({
      next: (response) => {
        this.isResending.set(false);
        this.success.set(response.message || 'Mã xác nhận mới đã được gửi.');
        this.digits.set(Array(OTP_LENGTH).fill(''));
        this.expirySeconds.set(OTP_VALIDITY_SECONDS);
        this.resendCooldown.set(RESEND_COOLDOWN_SECONDS);
        this.startTimer();
        this.focusBox(0);
      },
      error: (err) => {
        this.isResending.set(false);
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'Không thể gửi lại mã. Vui lòng thử lại sau.');
      }
    });
  }
}
