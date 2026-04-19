import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import { NotificationFilterRequest, NotificationListItemDto } from '../shared/models/notification.models';
import { toHttpParams, unwrapApiResponse, unwrapApiVoid } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/notifications');

  getMyPaged(filter: NotificationFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      unreadOnly: filter.unreadOnly ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<NotificationListItemDto>>>(`${this.base}/my`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getMyUnreadCount() {
    return this.http
      .get<ApiResponse<number>>(`${this.base}/my/unread-count`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  markRead(id: string) {
    return this.http
      .post<ApiResponse>(`${this.base}/${id}/mark-read`, {})
      .pipe(map((r) => unwrapApiVoid(r)));
  }

  markAllRead() {
    return this.http
      .post<ApiResponse>(`${this.base}/my/mark-all-read`, {})
      .pipe(map((r) => unwrapApiVoid(r)));
  }
}
