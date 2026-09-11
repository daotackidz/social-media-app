import { Component, Input } from '@angular/core';

import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
import { ProfileHighlight } from '../../models/profile.models';

@Component({
  selector: 'app-profile-highlights',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './profile-highlights.component.html',
  styleUrl: './profile-highlights.component.scss'
})
export class ProfileHighlightsComponent {
  @Input() highlights: ProfileHighlight[] = [];
  @Input() isCurrentUser = false;
}
