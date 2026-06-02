import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './app-input.component.html',
  styleUrl: './app-input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppInputComponent),
      multi: true,
    },
  ],
})
export class AppInputComponent {
  @Input() label: string = '';
  @Input() type: string = 'text';
  @Input() maxLength: number | null = null;
  @Input() autocomplete: string = 'off';
  @Input() errorMessage: string = '';
  @Input() control!: FormControl;
  @Input() fieldClass: string = '';
  @Input() prefixIcon: string = '';
  @Input() required: boolean = false;
  @Input() showTopLabel: boolean = true;

  showPassword = false;

  get inputType(): string {
    return this.type === 'password' && this.showPassword ? 'text' : this.type;
  }

  get showTogglePassword(): boolean {
    return this.type === 'password';
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}
