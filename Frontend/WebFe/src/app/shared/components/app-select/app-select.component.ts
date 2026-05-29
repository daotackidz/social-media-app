import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

export interface SelectOption {
  value: any;
  label: string;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule, MatIconModule],
  templateUrl: './app-select.component.html',
  styleUrl: './app-select.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppSelectComponent),
      multi: true
    }
  ]
})
export class AppSelectComponent {
  @Input() label: string = '';
  @Input() options: SelectOption[] = [];
  @Input() errorMessage: string = '';
  @Input() control!: FormControl;
  @Input() fieldClass: string = '';
  @Input() prefixIcon: string = '';
}
