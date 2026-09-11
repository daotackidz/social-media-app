export interface ProfileStats {
  postsCount: number;
  followersCount: number;
  followingCount: number;
}

export interface UserProfileSummary {
  username: string;
  fullName: string;
  bio: string;
  websiteUrl?: string;
  avatarUrl?: string;
  /** Fallback when there's no avatarUrl — deterministic per-username, same convention as the rest of the app. */
  avatarColor: string;
  avatarInitial: string;
  verified: boolean;
  /** True when the signed-in user is viewing their own profile. */
  isCurrentUser: boolean;
  isFollowing: boolean;
  /** Username of a mutual/notable follower shown as "Followed by {name}". */
  followedByUsername?: string;
  stats: ProfileStats;
}

export interface ProfileHighlight {
  id: string;
  title: string;
  coverUrl?: string;
  avatarColor: string;
  avatarInitial: string;
}

export type ProfileTab = 'posts' | 'reels' | 'tagged';

export type ProfilePostType = 'image' | 'video' | 'carousel';

export interface ProfilePost {
  id: string;
  type: ProfilePostType;
  coverUrl?: string;
  avatarColor: string;
  likeCount: number;
  commentCount: number;
}

export interface FollowResult {
  isFollowing: boolean;
  followersCount: number;
}
