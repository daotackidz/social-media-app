import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponse } from '../core/api';

export interface SearchUser {
  userId: string;
  username: string;
  fullName: string;
  avatarUrl?: string | null;
  verified: boolean;
  isFollowing: boolean;
}

export interface SearchHistoryItem {
  id: string;
  user: SearchUser;
}

/**
 * Backs the search popup — mirrors Backend/Social.WebApi/Controllers/SearchController.cs.
 * History entries are soft-deleted server-side (RecordStatusId = Deleted), so
 * delete/clear here just tell the API which rows to hide, nothing is erased.
 */
@Injectable({ providedIn: 'root' })
export class SearchService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/search';

  searchUsers(query: string): Observable<ApiResponse<SearchUser[]>> {
    return this.http.get<ApiResponse<SearchUser[]>>(`${this.baseUrl}/users`, { params: { q: query } });
  }

  getHistory(): Observable<ApiResponse<SearchHistoryItem[]>> {
    return this.http.get<ApiResponse<SearchHistoryItem[]>>(`${this.baseUrl}/history`);
  }

  addHistory(targetUserId: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/history`, { targetUserId });
  }

  deleteHistory(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/history/${id}`);
  }

  clearHistory(): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/history`);
  }
}
