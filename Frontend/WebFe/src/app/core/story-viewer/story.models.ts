/** One playable story — a slide in one owner's reel inside the story viewer. */
export interface StoryDetail {
  id: string;
  userId: string;
  username: string;
  avatarUrl?: string;
  mediaUrl: string;
  mediaType: 'image' | 'video';
  caption: string;
  isAiGenerated: boolean;
  createdDate: string;
  likeCount: number;
  viewCount: number;
  /** Whether the signed-in user currently has an active like on this story. */
  isLiked: boolean;
  /** True when the signed-in user is this story's owner — hides like/reply and exposes the viewer list instead. */
  isOwnStory: boolean;
}

/** One row of a story's viewer list — visible only to the story's own owner. */
export interface StoryViewerInfo {
  userId: string;
  username: string;
  fullName?: string;
  avatarUrl?: string;
  viewedDate: string;
  liked: boolean;
}
