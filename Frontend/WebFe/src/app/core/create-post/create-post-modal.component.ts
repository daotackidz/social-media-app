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
  /** Auto-captured first frame, for videos only — becomes the post's cover image (grid thumbnail, feed <video poster>) since a raw video URL can't be used as an <img src>. */
  thumbnailBlob?: Blob;
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

    const thumbnail = selected.find((s) => s.isVideo && s.thumbnailBlob)?.thumbnailBlob;

    this.createPostService.createPost(selected.map((s) => s.file), this.caption(), this.aiLabel(), this.altTexts(), thumbnail).subscribe({
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

    for (const entry of accepted) {
      if (entry.isVideo) this.captureVideoThumbnail(entry);
    }
  }

  /**
   * Grabs the video's first frame into a JPEG blob so the post has a real
   * cover image (a raw video URL can't be used as an <img src> — that's why
   * video posts showed a broken thumbnail in the grid before this existed).
   * Runs off-screen; updates the matching SelectedFile once the frame is ready.
   */
  private captureVideoThumbnail(entry: SelectedFile): void {
    const video = document.createElement('video');
    video.preload = 'metadata';
    video.muted = true;
    video.playsInline = true;
    video.src = entry.previewUrl;

    const cleanup = () => {
      video.removeAttribute('src');
      video.load();
    };

    video.addEventListener(
      'loadeddata',
      () => {
        // A hair past 0 avoids some encoders' all-black very first frame.
        video.currentTime = Math.min(0.1, (video.duration || 1) / 10);
      },
      { once: true }
    );

    video.addEventListener(
      'seeked',
      () => {
        const canvas = document.createElement('canvas');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        const ctx = canvas.getContext('2d');
        if (!ctx || canvas.width === 0 || canvas.height === 0) {
          cleanup();
          return;
        }
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        canvas.toBlob(
          (blob) => {
            cleanup();
            if (!blob) return;
            this.files.update((current) => current.map((f) => (f === entry ? { ...f, thumbnailBlob: blob } : f)));
          },
          'image/jpeg',
          0.85
        );
      },
      { once: true }
    );

    video.addEventListener('error', cleanup, { once: true });
  }

  ngOnDestroy(): void {
    this.closeSub.unsubscribe();
    for (const f of this.files()) URL.revokeObjectURL(f.previewUrl);
  }
}
