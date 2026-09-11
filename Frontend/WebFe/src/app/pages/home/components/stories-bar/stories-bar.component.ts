import { Component, Input } from '@angular/core';

import { StoryItem } from '../../models/feed.models';

@Component({
  selector: 'app-stories-bar',
  standalone: true,
  templateUrl: './stories-bar.component.html',
  styleUrl: './stories-bar.component.scss'
})
export class StoriesBarComponent {
  @Input() stories: StoryItem[] = [];
}
