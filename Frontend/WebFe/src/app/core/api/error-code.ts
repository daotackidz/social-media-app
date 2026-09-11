/**
 * Mirrors Backend/Social.Common/Constants/ErrorCode.cs member-for-member.
 * The backend serializes the enum as this exact string (JsonStringEnumConverter
 * in Program.cs), so keep the two lists in sync whenever either changes.
 *
 * Always branch application logic on this value, never on `message` — message
 * is user-facing display text and changes with the active language (/vn, /en).
 */
export type ErrorCode =
  | 'VALIDATION_ERROR'
  | 'INVALID_CREDENTIALS'
  | 'UNAUTHORIZED'
  | 'EMAIL_EXISTS'
  | 'USERNAME_EXISTS'
  | 'USERNAME_NOT_FOUND'
  | 'EMAIL_NOT_FOUND'
  | 'USER_NOT_FOUND'
  | 'PROFILE_NOT_FOUND'
  | 'PENDING_REGISTRATION_NOT_FOUND'
  | 'OTP_EXPIRED'
  | 'OTP_INVALID'
  | 'OTP_COOLDOWN'
  | 'PASSWORD_MISMATCH'
  | 'FILE_REQUIRED'
  | 'CONTAINER_REQUIRED'
  | 'FILE_NOT_FOUND'
  | 'SEARCH_HISTORY_NOT_FOUND'
  | 'INTERNAL_ERROR';
