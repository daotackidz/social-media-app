export interface PostComment {
  id: string;
  username: string;
  avatarUrl?: string;
  content: string;
  createdDate: string;
  likeCount: number;
  /** Whether the signed-in user currently has an active like on this comment. */
  isLiked: boolean;
  /** Number of replies — always 0 on a reply itself (replies are flat, never nested further). */
  replyCount: number;

  // --- Client-only UI state for the reply thread, populated/toggled locally as the user expands it ---
  /** Loaded replies, once fetched — undefined until the user has opened the thread at least once. */
  replies?: PostComment[];
  /** Whether the loaded replies are currently shown ("Ẩn câu trả lời" vs "Xem N câu trả lời"). */
  repliesExpanded?: boolean;
  /** Whether another page of replies exists beyond what's loaded in `replies`. */
  repliesHasMore?: boolean;
}

/** One row of the "Lượt thích" (likes list) popup. */
export interface PostLikeUser {
  userId: string;
  username: string;
  fullName?: string;
  avatarUrl?: string;
  isCurrentUser: boolean;
  isFollowing: boolean;
}

export interface PostDetail {
  id: string;
  username: string;
  verified: boolean;
  avatarUrl?: string;
  imageUrls: string[];
  videoUrl?: string;
  /** The video's captured cover frame, shown before playback starts. Only set alongside videoUrl. */
  posterUrl?: string;
  caption: string;
  edited: boolean;
  likeCount: number;
  /** Whether the signed-in user currently has an active like on this post. */
  isLiked: boolean;
  commentCount: number;
  createdDate: string;
  /** First page only (10 top-level comments) — more are paged in via PostDetailService.getComments as the panel scrolls. */
  comments: PostComment[];
  /** Whether more top-level comments exist beyond the initial page in `comments`. */
  commentsHasMore: boolean;
}
