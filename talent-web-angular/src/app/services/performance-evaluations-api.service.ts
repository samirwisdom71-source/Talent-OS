import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreatePerformanceEvaluationRequest,
  PerformanceEvaluationDto,
  PerformanceEvaluationFilterRequest,
  UpdatePerformanceEvaluationRequest,
} from '../shared/models/performance.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class PerformanceEvaluationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/performance-evaluations');

  getPaged(filter: PerformanceEvaluationFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<PerformanceEvaluationDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<PerformanceEvaluationDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreatePerformanceEvaluationRequest) {
    return this.http
      .post<ApiResponse<PerformanceEvaluationDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdatePerformanceEvaluationRequest) {
    return this.http
      .put<ApiResponse<PerformanceEvaluationDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
