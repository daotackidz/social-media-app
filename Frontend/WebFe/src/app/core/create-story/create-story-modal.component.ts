import { Component, ElementRef, HostListener, inject, OnDestroy, signal, viewChild } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../i18n/language.service';
import { TranslatePipe } from '../i18n/translate.pipe';
import { CreateStoryService } from './create-story.service';

const MAX_VIDEO_SECONDS = 15;

interface SelectedFile {
  file: File;
  previewUrl: string;
  isVideo: boolean;
}

type ModalStep = 'picker' | 'compose';

/** "Tin mới" — a trimmed-down version of CreatePostModalComponent: one file (image or video, ≤15s), a caption, and an AI label. */
@Component({
  selector: 'app-create-story-modal',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './create-story-modal.component.html',
  styleUrl: './create-story-modal.component.scss'
})
export class CreateStoryModalComponent implements OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<CreateStoryModalComponent>);
  private readonly createStoryService = inject(CreateStoryService);
  protected readonly authService = inject(AuthService);
  private readonly languageService = inject(LanguageService);

  readonly step = signal<ModalStep>('picker');
  readonly selected = signal<SelectedFile | null>(null);
  readonly caption = signal('');
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly dragActive = signal(false);
  readonly aiLabel = signal(false);

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

  readonly discardIntent = signal<'close' | 'back' | null>(null);
  private readonly closeSub: Subscription;

  constructor() {
    this.dialogRef.disableClose = true;
    this.closeSub = this.dialogRef.backdropClick().subscribe(() => this.requestClose());
    this.closeSub.add(
      this.dialogRef.keydownEvents().subscribe((event) => {
        if (event.key === 'Escape') this.requestClose();
      })
    );
  }

  get captionCounter(): string {
    const locale = this.languageService.lang() === 'vi' ? 'vi-VN' : 'en-US';
    return `${this.caption().length.toLocaleString(locale)}/${this.captionMaxLength.toLocaleString(locale)}`;
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addFile(input.files?.item(0) ?? null);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(false);
    this.addFile(event.dataTransfer?.files?.item(0) ?? null);
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

  removeFile(): void {
    const current = this.selected();
    if (!current) return;
    URL.revokeObjectURL(current.previewUrl);
    this.selected.set(null);
    this.step.set('picker');
  }

  private goBack(): void {
    const current = this.selected();
    if (current) URL.revokeObjectURL(current.previewUrl);
    this.selected.set(null);
    this.error.set('');
    this.step.set('picker');
  }

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
    const current = this.selected();
    if (!current || this.submitting()) return;

    this.submitting.set(true);
    this.error.set('');

    this.createStoryService.createStory(current.file, this.caption(), this.aiLabel()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.dialogRef.close();
      },
      error: () => {
        this.submitting.set(false);
        this.error.set('createStory.error');
      }
    });
  }

  close(): void {
    if (this.submitting()) return;
    this.dialogRef.close();
  }

  private requestClose(): void {
    if (this.submitting()) return;
    if (this.step() === 'compose') {
      this.discardIntent.set('close');
    } else {
      this.dialogRef.close();
    }
  }

  private addFile(file: File | null): void {
    if (!file) return;

    const isImage = file.type.startsWith('image/');
    const isVideo = file.type.startsWith('video/');
    if (!isImage && !isVideo) {
      this.error.set('createStory.invalidType');
      return;
    }

    const previewUrl = URL.createObjectURL(file);

    if (!isVideo) {
      this.error.set('');
      this.selected.set({ file, previewUrl, isVideo: false });
      this.step.set('compose');
      return;
    }

    // Videos must be checked against the 15s cap before they're accepted — done by
    // loading their metadata off-screen rather than trusting the file as-is.
    const probe = document.createElement('video');
    probe.preload = 'metadata';
    probe.src = previewUrl;
    probe.addEventListener(
      'loadedmetadata',
      () => {
        if (probe.duration > MAX_VIDEO_SECONDS + 0.5) {
          URL.revokeObjectURL(previewUrl);
          this.error.set('createStory.videoTooLong');
          return;
        }
        this.error.set('');
        this.selected.set({ file, previewUrl, isVideo: true });
        this.step.set('compose');
      },
      { once: true }
    );
    probe.addEventListener(
      'error',
      () => {
        URL.revokeObjectURL(previewUrl);
        this.error.set('createStory.invalidType');
      },
      { once: true }
    );
  }

  ngOnDestroy(): void {
    this.closeSub.unsubscribe();
    const current = this.selected();
    if (current) URL.revokeObjectURL(current.previewUrl);
  }
}
