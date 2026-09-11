import { ErrorCode } from './error-code';

/**
 * The one response envelope every backend endpoint returns — mirrors
 * Backend/Social.Data/Model/Response/Base/ApiResponse.cs. Every HTTP service
 * in this app should type its Observable as `ApiResponse<T>` so callers get
 * `data`, `message` and `errorCode` consistently instead of guessing the
 * shape per endpoint.
 */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T | null;
  statusCode: number;
  /** Machine-readable failure reason, null on success. Switch on this, not `message`. */
  errorCode?: ErrorCode | null;
  timestamp: string;
}
