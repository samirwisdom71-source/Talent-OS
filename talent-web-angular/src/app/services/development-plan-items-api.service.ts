import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateDevelopmentPlanItemRequest,
  DevelopmentPlanItemDto,
  DevelopmentPlanItemFilterRequest,
  UpdateDevelopmentPlanItemRequest,
  UpdateDevelopmentPlanItemProgressRequest,
} from '../shared/models/development-item.models';
import { toHttpParams, unwrapApiResponse, unwrapApiVoid } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class DevelopmentPlanItemsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/development-plan-items');

  getPaged(filter: DevelopmentPlanItemFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      developmentPlanId: filter.developmentPlanId,
    });
    return this.http
      .get<ApiResponse<PagedResult<DevelopmentPlanItemDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<DevelopmentPlanItemDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateDevelopmentPlanItemRequest) {
    return this.http
      .post<ApiResponse<DevelopmentPlanItemDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateDevelopmentPlanItemRequest) {
    return this.http
      .put<ApiResponse<DevelopmentPlanItemDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  updateProgress(id: string, body: UpdateDevelopmentPlanItemProgressRequest) {
    return this.http
      .post<ApiResponse<DevelopmentPlanItemDto>>(`${this.base}/${id}/update-progress`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  markCompleted(id: string) {
    return this.http
      .post<ApiResponse<DevelopmentPlanItemDto>>(`${this.base}/${id}/mark-completed`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  remove(id: string) {
    return this.http
      .delete<ApiResponse>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiVoid(r)));
  }
}
