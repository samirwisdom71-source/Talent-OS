import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateNotificationTemplateRequest,
  NotificationTemplateDto,
  NotificationTemplateFilterRequest,
  UpdateNotificationTemplateRequest,
} from '../shared/models/notification.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class NotificationTemplatesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/notification-templates');

  getPaged(filter: NotificationTemplateFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
      activeOnly: filter.activeOnly ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<NotificationTemplateDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<NotificationTemplateDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateNotificationTemplateRequest) {
    return this.http
      .post<ApiResponse<NotificationTemplateDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateNotificationTemplateRequest) {
    return this.http
      .put<ApiResponse<NotificationTemplateDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
