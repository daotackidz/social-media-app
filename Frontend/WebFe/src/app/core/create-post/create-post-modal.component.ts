import { Component, computed, ElementRef, HostListener, inject, OnDestroy, signal, viewChild } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../i18n/language.service';
import { TranslatePipe } from '../i18n/translate.pipe';
import { CreatePostService } from './create-post.service';

interface SelectedFile {
  file: File;
  previewUrl: string;
  isVideo: boolean;
}

type ModalStep = 'picker' | 'compose';

@Component({
  selector: 'app-create-post-modal',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './create-post-modal.component.html',
  styleUrl: './create-post-modal.component.scss'
})
export class CreatePostModalComponent implements OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<CreatePostModalComponent>);
  private readonly createPostService = inject(CreatePostService);
  protected readonly authService = inject(AuthService);
  private readonly languageService = inject(LanguageService);

  readonly step = signal<ModalStep>('picker');
  readonly files = signal<SelectedFile[]>([]);
  readonly activeIndex = signal(0);
  readonly caption = signal('');
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly dragActive = signal(false);

  /** Whether the post is labeled as containing AI-generated content ("AI info"). */
  readonly aiLabel = signal(false);
  readonly shareToExpanded = signal(false);
  readonly accessibilityExpanded = signal(false);

  /** Alt text per file index, sent to the backend keyed by each file's position. */
  readonly altTexts = signal<Record<number, string>>({});

  readonly captionMaxLength = 2200;

  readonly emojiPickerOpen = signal(false);
  readonly commonEmojis = [
    '😂', '😮', '😞', '😡', '👏', '🔥', '🎉', '💯',
    '❤️', '🤣', '🤗', '🤔', '☺️', '😊',
    '😍', '😘', '😭', '😅', '😁', '😉', '🙌', '🙏',
    '👍', '👎', '👋', '💪', '✨', '🎂', '😎', '🥳',
    '😢', '😱', '🤩', '🤯', '🙄', '😴', '🤤', '🤢',
    '💔', '💕', '⭐', '✅', '🌸', '🍀', '☀️', '🌙'
  ];
  private readonly captionInput = viewChild<ElementRef<HTMLTextAreaElement>>('captionInput');
  private readonly emojiPicker = viewChild<ElementRef<HTMLElement>>('emojiPicker');

  /** Set while a discard-confirmation is up, remembering what to do if the user confirms. */
  readonly discardIntent = signal<'close' | 'back' | null>(null);
  private readonly closeSub: Subscription;

  constructor() {
    // Editing a post is easy to lose by mis-clicking the backdrop or hitting Esc, so
    // route both through the same confirmation as the back button instead of closing outright.
    this.dialogRef.disableClose = true;
    this.closeSub = this.dialogRef.backdropClick().subscribe(() => this.requestClose());
    this.closeSub.add(
      this.dialogRef.keydownEvents().subscribe((event) => {
        if (event.key === 'Escape') this.requestClose();
      })
    );
  }

  readonly activeFile = computed(() => this.files()[this.activeIndex()] ?? null);

  readonly captionCounter = computed(() => {
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return `${this.caption().length.toLocaleString(locale)}/${this.captionMaxLength.toLocaleString(locale)}`;
  });

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addFiles(input.files);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(false);
    this.addFiles(event.dataTransfer?.files ?? null);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(true);
  }

  onDragLeave(): void {
    this.dragActive.set(false);
  }

  onCaptionInput(event: Event): void {
    this.caption.set((event.target as HTMLTextAreaElement).value);
  }

  currentAltText(): string {
    return this.altTexts()[this.activeIndex()] ?? '';
  }

  onAltTextInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const index = this.activeIndex();
    this.altTexts.update((prev) => ({ ...prev, [index]: value }));
  }

  toggleEmojiPicker(): void {
    this.emojiPickerOpen.update((v) => !v);
  }

  addEmoji(emoji: string): void {
    const textarea = this.captionInput()?.nativeElement;
    const start = textarea?.selectionStart ?? this.caption().length;
    const end = textarea?.selectionEnd ?? this.caption().length;
    const value = this.caption();
    const next = value.slice(0, start) + emoji + value.slice(end);
    this.caption.set(next);
    this.emojiPickerOpen.set(false);

    if (textarea) {
      queueMicrotask(() => {
        const cursor = start + emoji.length;
        textarea.focus();
        textarea.setSelectionRange(cursor, cursor);
      });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.emojiPickerOpen()) return;
    const target = event.target as Node;
    if (this.emojiPicker()?.nativeElement.contains(target)) return;
    this.emojiPickerOpen.set(false);
  }

  toggleShareTo(): void {
    this.shareToExpanded.update((v) => !v);
  }

  toggleAccessibility(): void {
    this.accessibilityExpanded.update((v) => !v);
  }

  prevFile(): void {
    const count = this.files().length;
    if (count < 2) return;
    this.activeIndex.update((i) => (i - 1 + count) % count);
  }

  nextFile(): void {
    const count = this.files().length;
    if (count < 2) return;
    this.activeIndex.update((i) => (i + 1) % count);
  }

  removeActiveFile(): void {
    const index = this.activeIndex();
    const removed = this.files()[index];
    if (!removed) return;

    URL.revokeObjectURL(removed.previewUrl);
    const next = this.files().filter((_, i) => i !== index);
    this.files.set(next);
    this.activeIndex.set(Math.min(index, Math.max(next.length - 1, 0)));

    if (next.length === 0) this.step.set('picker');
  }

  /** No crop/adjust step exists yet, so "back" just restarts the picker. */
  private goBack(): void {
    for (const f of this.files()) URL.revokeObjectURL(f.previewUrl);
    this.files.set([]);
    this.activeIndex.set(0);
    this.error.set('');
    this.step.set('picker');
  }

  /** Editing has started (there's media and/or a caption to lose), so confirm first. */
  onBackClick(): void {
    if (this.submitting()) return;
    this.discardIntent.set('back');
  }

  confirmDiscard(): void {
    const intent = this.discardIntent();
    this.discardIntent.set(null);
    if (intent === 'back') this.goBack();
    else if (intent === 'close') this.dialogRef.close();
  }

  cancelDiscard(): void {
    this.discardIntent.set(null);
  }

  share(): void {
    const selected = this.files();
    if (selected.length === 0 || this.submitting()) return;

    this.setSubmitting(true);
    this.error.set('');

    this.createPostService.createPost(selected.map((s) => s.file), this.caption(), this.aiLabel(), this.altTexts()).subscribe({
      next: () => {
        this.setSubmitting(false);
        this.dialogRef.close();
      },
      error: () => {
        this.setSubmitting(false);
        this.error.set('create.error');
      }
    });
  }

  close(): void {
    if (this.submitting()) return;
    this.dialogRef.close();
  }

  /** Backdrop click / Esc while composing: confirm before discarding, like the back button. */
  private requestClose(): void {
    if (this.submitting()) return;
    if (this.step() === 'compose') {
      this.discardIntent.set('close');
    } else {
      this.dialogRef.close();
    }
  }

  private setSubmitting(value: boolean): void {
    this.submitting.set(value);
  }

  private addFiles(fileList: FileList | null): void {
    if (!fileList || fileList.length === 0) return;

    const accepted: SelectedFile[] = [];
    for (const file of Array.from(fileList)) {
      const isImage = file.type.startsWith('image/');
      const isVideo = file.type.startsWith('video/');
      if (!isImage && !isVideo) continue;
      accepted.push({ file, previewUrl: URL.createObjectURL(file), isVideo });
    }

    if (accepted.length === 0) {
      this.error.set('create.invalidType');
      return;
    }

    this.error.set('');
    const wasEmpty = this.files().length === 0;
    this.files.update((current) => [...current, ...accepted]);
    if (wasEmpty) this.activeIndex.set(0);
    this.step.set('compose');
  }

  ngOnDestroy(): void {
    this.closeSub.unsubscribe();
    for (const f of this.files()) URL.revokeObjectURL(f.previewUrl);
  }
}
