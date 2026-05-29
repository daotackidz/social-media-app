# Custom Material UI Input - Hướng dẫn Chi Tiết

## 1️⃣ CSS VARIABLES (Dễ nhất, khuyến khích)

Sửa `variables.scss` để áp dụng toàn bộ app:

```scss
:root {
  // Custom input styling
  --mat-input-border-color: #e5e7eb;
  --mat-input-focus-border-color: #4338ca;
  --mat-input-error-border-color: #b91c1c;
  --mat-input-text-color: #1f2937;
  --mat-input-label-color: #6b7280;
  --mat-input-background: #f9fafb;
  
  // Border radius
  --mat-input-border-radius: 8px;
  
  // Padding
  --mat-input-padding: 12px 16px;
}
```

Sau đó dùng trong component SCSS:

```scss
// app-input.component.scss
::ng-deep .mat-mdc-form-field {
  .mat-mdc-form-field-focus-overlay {
    background-color: rgba(67, 56, 202, 0.04);
  }

  .mat-mdc-text-field-wrapper {
    padding-bottom: 0;
  }
}

::ng-deep .mat-mdc-form-field-infix {
  padding-top: 12px;
  padding-bottom: 12px;
}

::ng-deep .mdc-text-field {
  background-color: var(--mat-input-background);
  border-radius: var(--mat-input-border-radius);
}
```

---

## 2️⃣ SCSS Theming (Cấp cao nhất, tùy chỉnh sâu)

Update `styles.scss` với custom theme:

```scss
@use '@angular/material' as mat;
@use 'sass:map';

// Define custom palette
$custom-primary: mat.define-palette(
  (
    0: #ffffff,
    10: #f3f1ff,
    20: #e8e4ff,
    25: #e1dcff,
    30: #dcd5ff,
    35: #d7ceff,
    40: #cfc7ff,
    50: #bbb2ff,
    60: #a39eff,
    70: #8a7fff,
    80: #7263ff,
    90: #6b47ff,
    95: #f3f1ff,
    99: #fffbfe,
    100: #ffffff,
  ),
  90,  // default
  80,  // lighter
  100  // darker
);

html {
  @include mat.theme(
    (
      color: (
        primary: $custom-primary,
      ),
    )
  );
}
```

---

## 3️⃣ Inline Style trong Template

```html
<mat-form-field 
  appearance="outline" 
  class="custom-input"
  [style.--input-color]="'#4338ca'"
>
  <mat-label>Email</mat-label>
  <input matInput />
</mat-form-field>
```

---

## 4️⃣ Component-level Customization (AppInputComponent)

Update component để hỗ trợ custom styling:

```typescript
import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-input',
  template: `
    <mat-form-field 
      [appearance]="appearance"
      [class]="fieldClass"
      [style.--input-border-color]="borderColor"
      [style.--input-focus-color]="focusColor"
    >
      <mat-label>{{ label }}</mat-label>
      <input matInput
        [type]="type"
        [formControl]="control"
        [style.--input-text-size]="fontSize"
      />
      @if (control.invalid && control.touched) {
        <mat-error>{{ errorMessage }}</mat-error>
      }
    </mat-form-field>
  `,
  styles: [`
    ::ng-deep {
      .mat-mdc-form-field {
        --mdc-theme-primary: var(--input-focus-color, #4338ca);
      }
      
      .mdc-text-field {
        border-radius: 8px;
      }
    }
  `]
})
export class AppInputComponent {
  @Input() label: string = '';
  @Input() type: string = 'text';
  @Input() control!: FormControl;
  @Input() errorMessage: string = '';
  @Input() fieldClass: string = '';
  @Input() appearance: 'fill' | 'outline' = 'outline';
  @Input() borderColor: string = '#e5e7eb';
  @Input() focusColor: string = '#4338ca';
  @Input() fontSize: string = '1rem';
}
```

---

## 5️⃣ SCSS Mixin (Reusable)

Tạo file `_material-input-theme.scss`:

```scss
@mixin custom-input-theme($primary, $accent, $warn) {
  ::ng-deep {
    .custom-input {
      .mdc-text-field {
        background-color: rgba(0, 0, 0, 0.02);
        border-radius: 8px;
      }

      .mat-mdc-form-field-focus-overlay {
        background-color: rgba($primary, 0.08);
      }

      &.mat-focused {
        .mdc-notched-outline__leading,
        .mdc-notched-outline__notch,
        .mdc-notched-outline__trailing {
          border-color: $primary;
          border-width: 2px;
        }
      }

      &.mat-form-field-invalid {
        .mdc-notched-outline__leading,
        .mdc-notched-outline__notch,
        .mdc-notched-outline__trailing {
          border-color: $warn;
        }
      }
    }
  }
}

// Usage
@include custom-input-theme(#4338ca, #00bcd4, #b91c1c);
```

---

## 6️⃣ Global Material Styles (styles.scss)

```scss
// Override Material MDC styles globally
::ng-deep {
  // All form fields
  .mat-mdc-form-field {
    width: 100%;
  }

  // Form field labels
  .mat-mdc-form-field-label {
    color: var(--mat-input-label-color) !important;
    font-size: 14px;
  }

  // Input text
  .mat-mdc-input-element {
    color: var(--mat-input-text-color) !important;
    font-size: 16px;
    letter-spacing: 0;
  }

  // Text field container
  .mdc-text-field {
    background-color: var(--mat-input-background);
    border-radius: var(--mat-input-border-radius);
  }

  // Focused state
  .mat-mdc-form-field.mat-focused {
    .mdc-notched-outline__leading,
    .mdc-notched-outline__notch,
    .mdc-notched-outline__trailing {
      border-color: var(--mat-input-focus-border-color) !important;
      border-width: 2px;
    }
  }

  // Error state
  .mat-form-field-invalid {
    .mdc-notched-outline__leading,
    .mdc-notched-outline__notch,
    .mdc-notched-outline__trailing {
      border-color: var(--mat-input-error-border-color) !important;
    }
  }

  // Error message
  .mat-error {
    font-size: 12px;
    color: var(--mat-input-error-border-color);
  }

  // Hint text
  .mat-mdc-form-field-hint {
    color: var(--mat-input-label-color);
    font-size: 12px;
  }
}
```

---

## 7️⃣ Appearance Modes Khác Nhau

```html
<!-- outline (mặc định) -->
<mat-form-field appearance="outline">
  <mat-label>Standard</mat-label>
  <input matInput />
</mat-form-field>

<!-- fill -->
<mat-form-field appearance="fill">
  <mat-label>Filled</mat-label>
  <input matInput />
</mat-form-field>

<!-- Custom class cho mỗi kiểu -->
<mat-form-field appearance="outline" class="input-variant-primary">
  <mat-label>Primary Variant</mat-label>
  <input matInput />
</mat-form-field>
```

Styles cho variants:

```scss
.input-variant-primary::ng-deep {
  .mdc-text-field {
    background: linear-gradient(135deg, #f3f1ff 0%, #e8e4ff 100%);
  }
}

.input-variant-success::ng-deep {
  .mdc-notched-outline__leading,
  .mdc-notched-outline__notch,
  .mdc-notched-outline__trailing {
    border-color: #0f766e !important;
  }
}

.input-variant-error::ng-deep {
  .mdc-notched-outline__leading,
  .mdc-notched-outline__notch,
  .mdc-notched-outline__trailing {
    border-color: #b91c1c !important;
  }
}
```

---

## 8️⃣ Ví Dụ Thực Tế: Custom Dark Mode Input

```scss
// dark-input.component.scss
.dark-input-wrapper {
  ::ng-deep {
    .mat-mdc-form-field {
      .mdc-text-field {
        background-color: #1f2937;
        color: #f3f4f6;
      }

      .mat-mdc-form-field-label {
        color: #d1d5db !important;
      }

      .mat-mdc-input-element {
        color: #f3f4f6 !important;
        caret-color: #60a5fa;

        &::placeholder {
          color: #9ca3af;
        }
      }

      .mdc-notched-outline__leading,
      .mdc-notched-outline__notch,
      .mdc-notched-outline__trailing {
        border-color: #374151;
      }

      &.mat-focused {
        .mdc-notched-outline__leading,
        .mdc-notched-outline__notch,
        .mdc-notched-outline__trailing {
          border-color: #60a5fa;
          border-width: 2px;
        }
      }
    }
  }
}
```

Template:
```html
<div class="dark-input-wrapper">
  <mat-form-field appearance="outline">
    <mat-label>Dark Mode Input</mat-label>
    <input matInput placeholder="Enter text..." />
  </mat-form-field>
</div>
```

---

## 📋 Summary: Cách Tốt Nhất Để Custom

| Phương pháp | Độ khó | Khi nào dùng |
|-----------|--------|------------|
| CSS Variables | ⭐ Dễ | Customize toàn app |
| SCSS Global (::ng-deep) | ⭐⭐ Dễ | Custom default styles |
| Component Props | ⭐⭐ Dễ | Per-instance customization |
| Material Theming | ⭐⭐⭐ Khó | Toàn bộ design system |
| Mixin SCSS | ⭐⭐ Dễ | Reusable themes |

**Khuyến nghị:** Combine CSS Variables + Global SCSS để có flexibility tốt nhất!
