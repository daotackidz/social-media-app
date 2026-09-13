import { Component, HostListener, Input, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CreatePostService } from '../../../../core/create-post/create-post.service';
import { CreateStoryService } from '../../../../core/create-story/create-story.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { NotificationService } from '../../../../core/notifications/notification.service';
import { NotificationsPopupComponent } from '../notifications-popup/notifications-popup.component';
import { SearchPopupComponent } from '../search-popup/search-popup.component';

interface NavItem {
  icon: string;
  labelKey: string;
}

/** Collapsed (icon-only) rail width, in px — matches the Figma design. */
export const RAIL_COLLAPSED_WIDTH = 72;
/** Expanded (icon + label) rail width, in px — the previous always-on layout. */
export const RAIL_EXPANDED_WIDTH = 244;

@Component({
  selector: 'app-sidebar-nav',
  standalone: true,
  imports: [RouterLink, TranslatePipe, SearchPopupComponent, NotificationsPopupComponent],
  templateUrl: './sidebar-nav.component.html',
  styleUrl: './sidebar-nav.component.scss'
})
export class SidebarNavComponent {
  /** Shown next to "Profile" — first letter is used as the avatar initial when there's no avatarUrl. */
  @Input() currentUsername = '';
  @Input() avatarUrl?: string;

  private readonly createPostService = inject(CreatePostService);
  private readonly createStoryService = inject(CreateStoryService);
  readonly notificationService = inject(NotificationService);

  readonly createMenuOpen = signal(false);

  /** True while the pointer is over the rail — drives the hover-to-expand behavior. */
  private readonly hovered = signal(false);
  /** True while the search popup is open — forces the rail back to collapsed so the popup sits at a fixed offset. */
  readonly searchOpen = signal(false);
  /** Same idea, for the notifications popup. */
  readonly notificationsOpen = signal(false);

  readonly expanded = computed(() => this.hovered() && !this.searchOpen() && !this.notificationsOpen());
  readonly railWidth = computed(() => (this.expanded() ? RAIL_EXPANDED_WIDTH : RAIL_COLLAPSED_WIDTH));
  readonly collapsedWidth = RAIL_COLLAPSED_WIDTH;

  /** Items with no page built yet — rendered as inert (no navigation). */
  readonly staticItems: NavItem[] = [{ icon: '/assets/icons/nav-reels.svg', labelKey: 'nav.reels' }];

  constructor() {
    this.notificationService.refreshUnreadCount();
  }

  get avatarInitial(): string {
    return (this.currentUsername || '?').charAt(0).toUpperCase();
  }

  onRailEnter(): void {
    this.hovered.set(true);
  }

  onRailLeave(): void {
    this.hovered.set(false);
  }

  toggleSearch(event: MouseEvent): void {
    event.stopPropagation();
    this.notificationsOpen.set(false);
    this.searchOpen.update((v) => !v);
  }

  closeSearch(): void {
    this.searchOpen.set(false);
  }

  toggleNotifications(event: MouseEvent): void {
    event.stopPropagation();
    this.searchOpen.set(false);
    this.notificationsOpen.update((v) => !v);
  }

  closeNotifications(): void {
    if (!this.notificationsOpen()) return;
    this.notificationsOpen.set(false);
    // Reading/marking things happens inside the popup — resync the badge once it's closed.
    this.notificationService.refreshUnreadCount();
  }

  toggleCreateMenu(): void {
    this.createMenuOpen.update((v) => !v);
  }

  selectPost(): void {
    this.createMenuOpen.set(false);
    this.createPostService.open();
  }

  selectStory(): void {
    this.createMenuOpen.set(false);
    this.createStoryService.open();
  }

  @HostListener('document:click')
  closeOverlays(): void {
    this.createMenuOpen.set(false);
    this.searchOpen.set(false);
    this.closeNotifications();
  }
}
