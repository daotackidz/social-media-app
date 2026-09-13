import { Component, Input, inject } from '@angular/core';

import { CreateStoryService } from '../../../../core/create-story/create-story.service';
import { StoryViewerService } from '../../../../core/story-viewer/story-viewer.service';
import { CurrentUserSummary, StoryItem } from '../../models/feed.models';

@Component({
  selector: 'app-stories-bar',
  standalone: true,
  templateUrl: './stories-bar.component.html',
  styleUrl: './stories-bar.component.scss'
})
export class StoriesBarComponent {
  @Input() stories: StoryItem[] = [];
  @Input() currentUser: CurrentUserSummary = { username: '', fullName: '', avatarColor: '#405de6' };

  private readonly createStoryService = inject(CreateStoryService);
  private readonly storyViewerService = inject(StoryViewerService);

  get avatarInitial(): string {
    return (this.currentUser.fullName || this.currentUser.username || '?').charAt(0).toUpperCase();
  }

  /** The signed-in user's own ring, when they currently have an active story (it rides along in the same feed page as everyone they follow). */
  get ownStory(): StoryItem | undefined {
    return this.stories.find((s) => s.username === this.currentUser.username);
  }

  get otherStories(): StoryItem[] {
    return this.stories.filter((s) => s.username !== this.currentUser.username);
  }

  /** Ring order the viewer plays through — matches what's rendered left to right (own tile first, when present). */
  private get orderedUserIds(): string[] {
    const others = this.otherStories.map((s) => s.userId);
    return this.ownStory ? [this.ownStory.userId, ...others] : others;
  }

  /** Own ring: opens the viewer at your own story if you have one, otherwise starts a new one — same as tapping the "+" badge. */
  openOwnRing(): void {
    if (this.ownStory) {
      this.storyViewerService.open(this.orderedUserIds, 0);
    } else {
      this.createStoryService.open();
    }
  }

  openAddStory(event: MouseEvent): void {
    event.stopPropagation();
    this.createStoryService.open();
  }

  openStory(story: StoryItem): void {
    const ids = this.orderedUserIds;
    const index = ids.indexOf(story.userId);
    this.storyViewerService.open(ids, Math.max(0, index));
  }
}
