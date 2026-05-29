# Custom Form Components

## AppInputComponent

Replaces `mat-form-field` + `mat-input` for text inputs.

### Usage in Component:
```typescript
import { AppInputComponent } from '../../shared/components';
import { FormBuilder } from '@angular/forms';

export class MyComponent {
  private fb = inject(FormBuilder);
  
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  get email() {
    return this.form.get('email');
  }
}
```

### Usage in Template:
```html
<app-input
  [control]="email!"
  label="Email"
  type="email"
  autocomplete="email"
  maxLength="200"
  errorMessage="Vui lòng nhập email hợp lệ"
  fieldClass="custom-class"
></app-input>
```

### Input Properties:
- `control: FormControl` - The form control (required)
- `label: string` - Field label text
- `type: string` - Input type (text, email, password, number, etc.) - default: 'text'
- `autocomplete: string` - Autocomplete attribute - default: 'off'
- `maxLength: number | null` - Maximum characters
- `errorMessage: string` - Error message to show when invalid
- `fieldClass: string` - CSS class for custom styling

---

## AppSelectComponent

Replaces `mat-form-field` + `mat-select` for dropdown selects.

### Usage in Component:
```typescript
import { AppSelectComponent, SelectOption } from '../../shared/components';
import { FormBuilder } from '@angular/forms';

export class MyComponent {
  private fb = inject(FormBuilder);
  
  days: SelectOption[] = Array.from({ length: 31 }, (_, i) => ({
    value: i + 1,
    label: String(i + 1)
  }));

  form = this.fb.group({
    day: [null]
  });

  get day() {
    return this.form.get('day');
  }
}
```

### Usage in Template:
```html
<app-select
  [control]="day!"
  label="Ngày"
  [options]="days"
  errorMessage="Vui lòng chọn ngày"
  fieldClass="custom-class"
></app-select>
```

### Input Properties:
- `control: FormControl` - The form control (required)
- `label: string` - Field label text
- `options: SelectOption[]` - Array of {value, label} objects
- `errorMessage: string` - Error message to show when invalid
- `fieldClass: string` - CSS class for custom styling

### SelectOption Interface:
```typescript
interface SelectOption {
  value: any;
  label: string;
}
```

---

## Customization

Both components can be easily customized by:

1. **Styling**: Pass a `fieldClass` prop to add custom CSS classes
2. **Appearance**: Modify the `appearance="outline"` in the template
3. **Validation**: Handle validation in your form control and pass appropriate `errorMessage`
4. **Material Theme**: The components inherit Material theming automatically

### Example: Custom Styling
```html
<app-input
  [control]="field!"
  label="Full Name"
  fieldClass="full-width custom-input-dark"
></app-input>
```

Then add your custom styles in your component's SCSS:
```scss
.full-width {
  width: 100%;
}

.custom-input-dark {
  ::ng-deep .mat-mdc-form-field {
    background: #1e1e1e;
  }
}
```
