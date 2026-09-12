import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../core/api';

export interface EditableProfile {
  username: string;
  fullName: string;
  bio: string;
  /** No backend enum exists yet — 0 = unspecified, 1 = male, 2 = female, 3 = other. */
  gender: number;
  /** yyyy-MM-dd, ready for the day/month/year selects, or '' when unset. */
  dateOfBirth: string;
  avatarUrl?: string;
  /** Private account — new followers must be approved before they see posts or count as following. */
  isPrivate: boolean;
}

export interface UpdateProfilePayload {
  fullName: string;
  bio: string;
  gender: number;
  dateOfBirth: string;
  avatarFile: File | null;
  isPrivate: boolean;
}

export interface UpdateProfileResult {
  profile: EditableProfile;
  /** The backend's own success message (e.g. "Cập nhật hồ sơ thành công."). */
  message: string;
}

interface MeApiResponse {
  username?: string | null;
  fullName?: string | null;
  bio?: string | null;
  gender?: number | null;
  dateOfBirth?: string | null;
  avatarUrl?: string | null;
  isPrivate?: boolean | null;
}

/**
 * Matches Backend/Social.WebApi/Controllers/ProfileController.cs — the
 * signed-in user editing their own profile, as opposed to profile/services
 * /profile.service.ts, which reads any user's public profile page.
 */
@Injectable({ providedIn: 'root' })
export class EditProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/account';

  getMyProfile(): Observable<EditableProfile> {
    return this.http.get<ApiResponse<MeApiResponse>>(`${this.baseUrl}/me`).pipe(
      map((res) => this.toEditableProfile(res.data ?? {}))
    );
  }

  updateProfile(payload: UpdateProfilePayload): Observable<UpdateProfileResult> {
    const formData = new FormData();
    formData.append('FullName', payload.fullName);
    formData.append('Bio', payload.bio);
    formData.append('Gender', String(payload.gender));
    formData.append('IsPrivate', String(payload.isPrivate));
    if (payload.dateOfBirth) formData.append('DateOfBirth', payload.dateOfBirth);
    if (payload.avatarFile) formData.append('AvatarFile', payload.avatarFile, payload.avatarFile.name);

    return this.http.put<ApiResponse<MeApiResponse>>(`${this.baseUrl}/update-profile`, formData).pipe(
      map((res) => ({
        profile: this.toEditableProfile(res.data ?? {}),
        message: res.message
      }))
    );
  }

  private toEditableProfile(data: MeApiResponse): EditableProfile {
    return {
      username: data.username ?? '',
      fullName: data.fullName ?? '',
      bio: data.bio ?? '',
      gender: data.gender ?? 0,
      dateOfBirth: data.dateOfBirth ? data.dateOfBirth.substring(0, 10) : '',
      avatarUrl: data.avatarUrl ?? undefined,
      isPrivate: data.isPrivate ?? false
    };
  }
}
