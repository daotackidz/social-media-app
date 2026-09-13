/** Mirrors Backend/Social.Data/Model/Notification/Notifications.cs's NotificationType enum. */
export type NotificationKind =
  | 'Follow'
  | 'LikePost'
  | 'CommentPost'
  | 'ReplyComment'
  | 'LikeComment'
  | 'SharePost'
  | 'Mention'
  | 'FollowRequest'
  | 'FollowAccepted'
  | 'LikeStory'
  | 'CommentStory';

export interface NotificationActor {
  userId: string;
  username: string;
  avatarUrl?: string;
}

/**
 * One row of the "Thông báo" popup. Likes on the same post/story from several
 * people arrive already grouped (see NotificationsController.GroupRaw) — Actors
 * holds the most recent few, totalActorCount the full tally.
 */
export interface AppNotification {
  id: string;
  type: NotificationKind;
  actors: NotificationActor[];
  totalActorCount: number;
  commentPreview?: string;
  postId?: string;
  storyId?: string;
  thumbnailUrl?: string;
  isRead: boolean;
  createdDate: string;
  /** Only set for a single-actor "Follow" row — drives the Follow/Following button. */
  isFollowingActor?: boolean;
}
