import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreatePerformanceGoalRequest,
  PerformanceGoalDto,
  PerformanceGoalFilterRequest,
  UpdatePerformanceGoalRequest,
} from '../shared/models/performance.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class PerformanceGoalsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/performance-goals');

  getPaged(filter: PerformanceGoalFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<PerformanceGoalDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<PerformanceGoalDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreatePerformanceGoalRequest) {
    return this.http.post<ApiResponse<PerformanceGoalDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdatePerformanceGoalRequest) {
    return this.http
      .put<ApiResponse<PerformanceGoalDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
