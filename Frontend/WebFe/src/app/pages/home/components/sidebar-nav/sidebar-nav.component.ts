import { Component, HostListener, Input, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CreatePostService } from '../../../../core/create-post/create-post.service';
import { LanguageService } from '../../../../core/i18n/language.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
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
  imports: [RouterLink, TranslatePipe, SearchPopupComponent],
  templateUrl: './sidebar-nav.component.html',
  styleUrl: './sidebar-nav.component.scss'
})
export class SidebarNavComponent {
  /** Shown next to "Profile" — first letter is used as the avatar initial when there's no avatarUrl. */
  @Input() currentUsername = '';
  @Input() avatarUrl?: string;

  private readonly createPostService = inject(CreatePostService);
  private readonly languageService = inject(LanguageService);

  readonly createMenuOpen = signal(false);

  /** True while the pointer is over the rail — drives the hover-to-expand behavior. */
  private readonly hovered = signal(false);
  /** True while the search popup is open — forces the rail back to collapsed so the popup sits at a fixed offset. */
  readonly searchOpen = signal(false);

  readonly expanded = computed(() => this.hovered() && !this.searchOpen());
  readonly railWidth = computed(() => (this.expanded() ? RAIL_EXPANDED_WIDTH : RAIL_COLLAPSED_WIDTH));
  readonly collapsedWidth = RAIL_COLLAPSED_WIDTH;

  /** Items with no page built yet — rendered as inert (no navigation). Order matches the Figma rail. */
  readonly staticItems: NavItem[] = [
    { icon: '/assets/icons/nav-reels.svg', labelKey: 'nav.reels' },
    { icon: '/assets/icons/nav-messages.svg', labelKey: 'nav.messages' }
  ];

  /** Fixture flag — no notifications backend yet, so the unread dot is just switched on for the demo. */
  readonly hasUnreadNotifications = true;

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
    this.searchOpen.update((v) => !v);
  }

  closeSearch(): void {
    this.searchOpen.set(false);
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
    // No story feature yet — placeholder until Stories are modeled/built.
    window.alert(this.languageService.t('create.storyComingSoon'));
  }

  @HostListener('document:click')
  closeOverlays(): void {
    this.createMenuOpen.set(false);
    this.searchOpen.set(false);
  }
}
