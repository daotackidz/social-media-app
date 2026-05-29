# Material Input Customization - Quick Reference

## 🎨 Các thuộc tính có thể custom

### Text & Label
```scss
// Label color & size
.mat-mdc-form-field-label {
  color: #6b7280 !important;
  font-size: 13px !important;
  font-weight: 500 !important;
}

// Input text color & size
.mat-mdc-input-element {
  color: #1f2937 !important;
  font-size: 15px !important;
  padding: 12px 16px !important;
}
```

### Border & Outline
```scss
// Border color
.mdc-notched-outline__leading,
.mdc-notched-outline__notch,
.mdc-notched-outline__trailing {
  border-color: #e5e7eb !important;
  border-width: 1px;
}

// Border color khi focus
&.mat-focused .mdc-notched-outline__leading,
&.mat-focused .mdc-notched-outline__notch,
&.mat-focused .mdc-notched-outline__trailing {
  border-color: #4338ca !important;
  border-width: 2px;
}
```

### Background
```scss
// Background color
.mdc-text-field {
  background-color: #f9fafb;
  border-radius: 8px;
}

// Gradient background
&.input-gradient .mdc-text-field {
  background: linear-gradient(135deg, #f3f1ff 0%, #e8e4ff 100%);
}
```

### States

**Hover:**
```scss
&:hover .mdc-text-field {
  background-color: #f3f4f6;
}
```

**Focus:**
```scss
&.mat-focused {
  .mat-mdc-form-field-label {
    color: #4338ca !important;
  }
  
  .mat-mdc-form-field-focus-overlay {
    background-color: rgba(67, 56, 202, 0.04) !important;
  }
}
```

**Disabled:**
```scss
&.mat-form-field-disabled {
  .mdc-text-field {
    background-color: #f3f4f6;
    opacity: 0.6;
  }
  
  .mat-mdc-input-element {
    color: #9ca3af !important;
  }
}
```

**Error:**
```scss
&.mat-form-field-invalid {
  .mdc-notched-outline__leading,
  .mdc-notched-outline__notch,
  .mdc-notched-outline__trailing {
    border-color: #b91c1c !important;
  }
  
  .mat-error {
    color: #b91c1c !important;
    font-size: 12px !important;
  }
}
```

---

## 🚀 Cách áp dụng nhanh nhất

### 1. **Dùng CSS Variables (khuyến khích)**

`variables.scss`:
```scss
:root {
  --input-primary-color: #4338ca;
  --input-text-color: #1f2937;
  --input-bg: #f9fafb;
  --input-border: #e5e7eb;
  --input-border-radius: 8px;
  --input-error-color: #b91c1c;
}
```

Dùng trong component:
```scss
::ng-deep {
  .mat-mdc-form-field {
    --mdc-theme-primary: var(--input-primary-color);
  }
  
  .mdc-text-field {
    background-color: var(--input-bg);
    border-radius: var(--input-border-radius);
  }
}
```

### 2. **Import global styles**

Thêm vào `styles.scss`:
```scss
@use 'app/shared/components/material-input-styles';
```

### 3. **Áp dụng variant class**

Template:
```html
<!-- Default -->
<mat-form-field appearance="outline">
  <mat-label>Normal</mat-label>
  <input matInput />
</mat-form-field>

<!-- Filled -->
<mat-form-field appearance="outline" class="input-filled">
  <mat-label>Filled</mat-label>
  <input matInput />
</mat-form-field>

<!-- Small -->
<mat-form-field appearance="outline" class="input-dense">
  <mat-label>Small</mat-label>
  <input matInput />
</mat-form-field>

<!-- Large -->
<mat-form-field appearance="outline" class="input-large">
  <mat-label>Large</mat-label>
  <input matInput />
</mat-form-field>

<!-- Success -->
<mat-form-field appearance="outline" class="input-success">
  <mat-label>Success</mat-label>
  <input matInput />
</mat-form-field>
```

---

## 🎯 Common Customization Examples

### **Ví dụ 1: Instagram-style Input**
```scss
.instagram-input {
  ::ng-deep {
    .mat-mdc-form-field {
      .mdc-text-field {
        background-color: #f9f9f9;
        border-radius: 6px;
      }

      .mat-mdc-input-element {
        font-size: 14px;
        padding: 10px 14px;
      }

      .mdc-notched-outline__leading,
      .mdc-notched-outline__notch,
      .mdc-notched-outline__trailing {
        border-color: #ddd !important;
      }

      &.mat-focused {
        .mdc-notched-outline__leading,
        .mdc-notched-outline__notch,
        .mdc-notched-outline__trailing {
          border-color: #000 !important;
          border-width: 1px;
        }
      }
    }
  }
}
```

### **Ví dụ 2: Minimal Style**
```scss
.minimal-input {
  ::ng-deep {
    .mat-mdc-form-field {
      .mdc-text-field {
        background-color: transparent;
        border-bottom: 2px solid #e5e7eb;
      }

      .mat-mdc-input-element {
        padding: 8px 0;
      }

      .mdc-notched-outline {
        display: none;
      }

      &.mat-focused .mdc-text-field {
        border-bottom-color: #4338ca;
      }
    }
  }
}
```

### **Ví dụ 3: Rounded Pill Style**
```scss
.pill-input {
  ::ng-deep {
    .mat-mdc-form-field {
      .mdc-text-field {
        background-color: #f3f4f6;
        border-radius: 24px;
        padding: 0 20px;
      }

      .mat-mdc-input-element {
        padding: 14px 0;
      }

      .mdc-notched-outline__leading {
        border-radius: 24px 0 0 24px;
      }

      .mdc-notched-outline__trailing {
        border-radius: 0 24px 24px 0;
      }
    }
  }
}
```

### **Ví dụ 4: Gradient Border**
```scss
.gradient-border-input {
  position: relative;

  ::ng-deep {
    .mat-mdc-form-field {
      .mdc-text-field {
        background: #f9fafb;
        position: relative;
        z-index: 1;

        &::before {
          content: '';
          position: absolute;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: linear-gradient(135deg, #4338ca, #60a5fa);
          border-radius: 8px;
          padding: 2px;
          z-index: -1;
        }
      }
    }
  }
}
```

---

## 📝 Debugging Tips

### Check current styles:
```bash
# Mở DevTools (F12)
# Chọn element → Inspect
# Tìm .mdc-text-field, .mat-mdc-input-element, v.v.
```

### View Material CSS Variables:
```javascript
// Console trong DevTools
getComputedStyle(document.querySelector('.mat-mdc-form-field')).getPropertyValue('--mdc-theme-primary')
```

### Override Material styles:
```scss
// Nếu style không được apply, dùng !important
.mat-mdc-form-field {
  ::ng-deep .mdc-text-field {
    background-color: red !important; // Force override
  }
}
```

---

## 📚 Material Appearance Modes

```html
<!-- outline (mặc định, tốt nhất) -->
<mat-form-field appearance="outline">
  <mat-label>Outline</mat-label>
  <input matInput />
</mat-form-field>

<!-- fill (cũ, không khuyến khích dùng) -->
<mat-form-field appearance="fill">
  <mat-label>Fill</mat-label>
  <input matInput />
</mat-form-field>
```

**Khuyến khích:** Luôn dùng `appearance="outline"` vì dễ custom hơn.

---

## ✅ Best Practices

1. **Dùng CSS Variables** để dễ maintain
2. **Không dùng `!important`** trừ khi thực sự cần thiết
3. **Test trên multiple browsers** (Chrome, Firefox, Safari)
4. **Prefix với `::ng-deep`** khi custom Material components
5. **Group related styles** thành variant classes
6. **Document custom styles** trong README

---

## 🔗 Useful Resources

- [Material Design Guide](https://material.io/design)
- [Angular Material Documentation](https://material.angular.dev)
- [MDC Web Components](https://github.com/material-components/material-components-web)
- [Material Theming Guide](https://material.angular.dev/guide/theming)
