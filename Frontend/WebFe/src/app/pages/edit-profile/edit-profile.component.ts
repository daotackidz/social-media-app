import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { FormControl, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { FooterComponent } from '../../shared/layout/footer/footer.component';
import { AppSelectComponent, SelectOption } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';
import { avatarColorFor, avatarInitialFor } from '../profile/utils/avatar-color';
import { EditProfileService } from './services/edit-profile.service';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe, AppSelectComponent, SidebarNavComponent, FooterComponent],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.scss'
})
export class EditProfileComponent implements OnDestroy {
  protected readonly authService = inject(AuthService);
  private readonly editProfileService = inject(EditProfileService);
  private readonly router = inject(Router);

  readonly bioMaxLength = 150;

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly successMessage = signal('');

  readonly username = signal('');
  readonly fullName = signal('');
  readonly bio = signal('');
  readonly gender = signal(0);
  readonly avatarUrl = signal<string | undefined>(undefined);
  readonly isPrivate = signal(false);

  /** Same day/month/year split as the register page, instead of a native date input. */
  readonly day = new FormControl<number | null>(null);
  readonly month = new FormControl<number | null>(null);
  readonly year = new FormControl<number | null>(null);

  readonly dayOptions: SelectOption[] = Array.from({ length: 31 }, (_, i) => ({ value: i + 1, label: String(i + 1) }));
  readonly monthOptions: SelectOption[] = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }));
  readonly yearOptions: SelectOption[] = Array.from({ length: 100 }, (_, i) => ({
    value: new Date().getFullYear() - i,
    label: String(new Date().getFullYear() - i)
  }));

  /** Client-side preview of a newly-picked file, kept separate from avatarUrl
   *  so it can be revoked on destroy/replacement without touching real data. */
  readonly avatarPreviewUrl = signal<string | null>(null);
  private pendingAvatarFile: File | null = null;

  readonly avatarColor = computed(() => avatarColorFor(this.username() || 'user'));
  readonly avatarInitial = computed(() => avatarInitialFor(this.fullName() || this.username()));

  /** For the sidebar's own avatar/link — same fallback as profile.component.ts. */
  readonly sidebarUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });
  readonly sidebarAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  constructor() {
    this.editProfileService.getMyProfile().subscribe({
      next: (profile) => {
        this.username.set(profile.username);
        this.fullName.set(profile.fullName);
        this.bio.set(profile.bio);
        this.gender.set(profile.gender);
        this.avatarUrl.set(profile.avatarUrl);
        this.isPrivate.set(profile.isPrivate);
        this.setDateOfBirth(profile.dateOfBirth);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('editProfile.error');
        this.loading.set(false);
      }
    });
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !file.type.startsWith('image/')) return;

    this.revokePreview();
    this.pendingAvatarFile = file;
    this.avatarPreviewUrl.set(URL.createObjectURL(file));
  }

  onFullNameInput(event: Event): void {
    this.fullName.set((event.target as HTMLInputElement).value);
  }

  onBioInput(event: Event): void {
    this.bio.set((event.target as HTMLTextAreaElement).value);
  }

  onGenderChange(event: Event): void {
    this.gender.set(Number((event.target as HTMLSelectElement).value));
  }

  toggleIsPrivate(): void {
    this.isPrivate.update((v) => !v);
  }

  /** Same rule as the register page: all three or none. */
  dobPartial(): boolean {
    const { value: day } = this.day;
    const { value: month } = this.month;
    const { value: year } = this.year;
    return (!!day || !!month || !!year) && !(day && month && year);
  }

  save(): void {
    if (this.saving()) return;

    if (this.dobPartial()) {
      this.error.set('editProfile.dobPartial');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.successMessage.set('');

    this.editProfileService.updateProfile({
      fullName: this.fullName().trim(),
      bio: this.bio().trim(),
      gender: this.gender(),
      dateOfBirth: this.formatDateOfBirth(),
      avatarFile: this.pendingAvatarFile,
      isPrivate: this.isPrivate()
    }).subscribe({
      next: ({ profile, message }) => {
        this.successMessage.set(message || 'editProfile.success');
        this.avatarUrl.set(profile.avatarUrl);
        this.revokePreview();
        this.authService.updateCachedProfile(profile.fullName, profile.avatarUrl ?? null);
        this.saving.set(false);

        const username = profile.username || this.sidebarUsername();
        this.router.navigate(['/profile', username]);
      },
      error: (err) => {
        this.saving.set(false);
        const apiError = apiErrorOf(err);
        this.error.set(apiError?.message || 'editProfile.error');
      }
    });
  }

  ngOnDestroy(): void {
    this.revokePreview();
  }

  private setDateOfBirth(value: string): void {
    if (!value) return;
    const [year, month, day] = value.split('-').map(Number);
    if (!year || !month || !day) return;
    this.day.setValue(day);
    this.month.setValue(month);
    this.year.setValue(year);
  }

  private formatDateOfBirth(): string {
    const { value: day } = this.day;
    const { value: month } = this.month;
    const { value: year } = this.year;
    if (!day || !month || !year) return '';
    return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
  }

  private revokePreview(): void {
    const preview = this.avatarPreviewUrl();
    if (preview) URL.revokeObjectURL(preview);
    this.avatarPreviewUrl.set(null);
    this.pendingAvatarFile = null;
  }
}
