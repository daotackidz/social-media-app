import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

import { FeedPost, StoryItem, SuggestedUser } from '../models/feed.models';

const MOCK_STORIES: StoryItem[] = [
  { id: 's1', username: 'itsdougthepug', avatarColor: '#f5a623', avatarInitial: 'D', viewed: false },
  { id: 's2', username: 'openaidalle', avatarColor: '#111827', avatarInitial: 'O', viewed: false },
  { id: 's3', username: 'lewishamilton', avatarColor: '#2563eb', avatarInitial: 'L', viewed: false },
  { id: 's4', username: 'wahab.xyz', avatarColor: '#0ea5e9', avatarInitial: 'W', viewed: true },
  { id: 's5', username: 'defavours', avatarColor: '#9333ea', avatarInitial: 'D', viewed: false },
  { id: 's6', username: 'mkbhd', avatarColor: '#dc2626', avatarInitial: 'M', viewed: true }
];

const MOCK_POSTS: FeedPost[] = [
  {
    id: 'p1',
    username: 'lewishamilton',
    verified: true,
    avatarColor: '#2563eb',
    avatarInitial: 'L',
    postedAgoMinutes: 5 * 60,
    likes: 741368,
    caption: 'Parabéns Ayrton, minha inspiração sempre 🇧🇷💫',
    commentsCount: 13384,
    comments: []
  },
  {
    id: 'p2',
    username: 'kurzgesagt',
    verified: true,
    avatarColor: '#059669',
    avatarInitial: 'K',
    postedAgoMinutes: 8 * 60,
    likes: 6724,
    caption: 'For every video we upload to YouTube we create different versions of the final thumbnail… more',
    commentsCount: 37,
    comments: [
      { username: 'kurzgesagt', text: 'Careful, please don’t burn the final thumbnail with too many fire emojis, we still need that one' },
      { username: 'kurzgesagt', text: '@xandrames2 🤗✨' }
    ]
  },
  {
    id: 'p3',
    username: 'discovery',
    verified: true,
    avatarColor: '#7c2d12',
    avatarInitial: 'D',
    postedAgoMinutes: 2 * 24 * 60,
    likes: 78780,
    caption: 'If you had to choose, where would you be the fastest: air, land, or sea?… more',
    commentsCount: 456,
    comments: []
  }
];

const MOCK_SUGGESTIONS_INLINE: SuggestedUser[] = [
  { id: 'u1', username: 'kirti_chadha', fullName: 'Kirti Chadha', reason: 'followsYou', avatarColor: '#be185d' },
  { id: 'u2', username: 'durgesh_nandini', fullName: 'Durgesh Nandini', reason: 'followedBy', reasonName: 'chirag_singla17', avatarColor: '#0891b2' },
  { id: 'u3', username: 'rohit_gupta', fullName: 'Rohit Gupta', reason: 'followedBy', reasonName: 'chirag_singla17', avatarColor: '#65a30d' }
];

const MOCK_SUGGESTIONS_PANEL: SuggestedUser[] = [
  { id: 'u4', username: 'imkir', reason: 'followsYou', avatarColor: '#334155' },
  { id: 'u5', username: 'organic__al', reason: 'followedBy', reasonName: 'chirag_singla17', avatarColor: '#78350f' },
  { id: 'u6', username: 'im_gr', reason: 'followedBy', reasonName: 'chirag_singla17', avatarColor: '#1e3a8a' },
  { id: 'u7', username: 'abh952', reason: 'followsYou', avatarColor: '#7c2d12' },
  { id: 'u8', username: 'sakbrl', reason: 'followsYou', avatarColor: '#4c1d95' }
];

/**
 * Each block on the home feed (stories / posts / suggestions) has its own
 * method here so it can be swapped from mock data to a real HTTP call one
 * method at a time — e.g. `getPosts()` becoming
 * `this.http.get<FeedPost[]>('/api/feed/posts')` — without touching the
 * components that consume it.
 */
@Injectable({ providedIn: 'root' })
export class FeedService {
  // Swap a method's body for an HttpClient call (e.g. `this.http.get<FeedPost[]>('/api/feed/posts')`)
  // to wire it up to the real backend; the components below don't need to change.

  getStories(): Observable<StoryItem[]> {
    return of(MOCK_STORIES);
  }

  getPosts(): Observable<FeedPost[]> {
    return of(MOCK_POSTS);
  }

  getInlineSuggestions(): Observable<SuggestedUser[]> {
    return of(MOCK_SUGGESTIONS_INLINE);
  }

  getPanelSuggestions(): Observable<SuggestedUser[]> {
    return of(MOCK_SUGGESTIONS_PANEL);
  }
}
