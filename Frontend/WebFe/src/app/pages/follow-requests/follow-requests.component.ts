import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { apiErrorOf } from '../../core/api';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { FooterComponent } from '../../shared/layout/footer/footer.component';
import { AuthService } from '../../services/auth.service';
import { SidebarNavComponent } from '../home/components/sidebar-nav/sidebar-nav.component';
import { FollowRequestItem } from '../profile/models/profile.models';
import { ProfileService } from '../profile/services/profile.service';

@Component({
  selector: 'app-follow-requests',
  standalone: true,
  imports: [RouterLink, TranslatePipe, SidebarNavComponent, FooterComponent],
  templateUrl: './follow-requests.component.html'
})
export class FollowRequestsComponent {
  private readonly authService = inject(AuthService);
  private readonly profileService = inject(ProfileService);

  readonly loading = signal(true);
  readonly requests = signal<FollowRequestItem[]>([]);
  readonly error = signal('');
  /** relationId of whichever row is mid-approve/reject, so only that row shows a spinner. */
  readonly actingOn = signal<string | null>(null);

  readonly sidebarUsername = computed(() => {
    const fallback = (this.authService.currentEmail() ?? '').split('@')[0] || 'you';
    return this.authService.currentUsername() ?? fallback;
  });
  readonly sidebarAvatarUrl = computed(() => this.authService.currentAvatarUrl() ?? undefined);

  constructor() {
    this.profileService.getFollowRequests().subscribe({
      next: (items) => {
        this.requests.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  approve(item: FollowRequestItem): void {
    if (this.actingOn()) return;
    this.actingOn.set(item.relationId);
    this.error.set('');

    this.profileService.approveFollowRequest(item.relationId).subscribe({
      next: () => {
        this.removeRequest(item.relationId);
        this.actingOn.set(null);
      },
      error: (err) => {
        this.actingOn.set(null);
        this.error.set(apiErrorOf(err)?.message || 'Không thể xử lý yêu cầu. Vui lòng thử lại.');
      }
    });
  }

  reject(item: FollowRequestItem): void {
    if (this.actingOn()) return;
    this.actingOn.set(item.relationId);
    this.error.set('');

    this.profileService.rejectFollowRequest(item.relationId).subscribe({
      next: () => {
        this.removeRequest(item.relationId);
        this.actingOn.set(null);
      },
      error: (err) => {
        this.actingOn.set(null);
        this.error.set(apiErrorOf(err)?.message || 'Không thể xử lý yêu cầu. Vui lòng thử lại.');
      }
    });
  }

  private removeRequest(relationId: string): void {
    this.requests.update((list) => list.filter((r) => r.relationId !== relationId));
  }
}
