export interface StoryItem {
  userId: string;
  username: string;
  /** Real photo, when the API has one — falls back to avatarColor/avatarInitial when absent. */
  avatarUrl?: string;
  avatarColor: string;
  avatarInitial: string;
  viewed: boolean;
}

export interface FeedPost {
  id: string;
  username: string;
  verified: boolean;
  avatarUrl?: string;
  avatarColor: string;
  avatarInitial: string;
  /** Every image attached to the post, in display order — empty for a video post (falls back to a plain avatarColor tile when there's neither). */
  imageUrls: string[];
  /** Set when the post is a video — takes priority over imageUrls when present. */
  videoUrl?: string;
  /** The video's captured cover frame, shown before playback starts. Only set alongside videoUrl. */
  posterUrl?: string;
  caption: string;
  likeCount: number;
  /** Whether the signed-in user currently has an active like on this post. */
  isLiked: boolean;
  commentCount: number;
  /** ISO datetime string from the API. */
  createdDate: string;
}

/** One page of an infinite-scroll list — mirrors Backend's PagedResponse<T>. */
export interface FeedPage<T> {
  items: T[];
  hasMore: boolean;
}

/** "new" = no follow-graph signal yet (fallback filler), just a plain "Suggested for you" caption. */
export type SuggestionReason = 'followsYou' | 'followedBy' | 'new';

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
