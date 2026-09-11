export interface StoryItem {
  id: string;
  username: string;
  /** Real photo, when the API has one — falls back to avatarColor/avatarInitial when absent. */
  avatarUrl?: string;
  avatarColor: string;
  avatarInitial: string;
  viewed: boolean;
}

export interface PostComment {
  username: string;
  text: string;
}

export interface FeedPost {
  id: string;
  username: string;
  verified: boolean;
  avatarUrl?: string;
  avatarColor: string;
  avatarInitial: string;
  postedAgoMinutes: number;
  likes: number;
  caption: string;
  commentsCount: number;
  comments: PostComment[];
}

export type SuggestionReason = 'followsYou' | 'followedBy';

export interface SuggestedUser {
  id: string;
  username: string;
  fullName?: string;
  reason: SuggestionReason;
  reasonName?: string;
  avatarUrl?: string;
  avatarColor: string;
}

export interface CurrentUserSummary {
  username: string;
  fullName: string;
  avatarUrl?: string;
  avatarColor: string;
}
