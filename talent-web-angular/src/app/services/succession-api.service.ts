import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateSuccessionPlanRequest,
  CriticalPositionDto,
  CriticalPositionFilterRequest,
  SuccessionPlanDto,
  SuccessionPlanFilterRequest,
} from '../shared/models/succession.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class SuccessionApiService {
  private readonly http = inject(HttpClient);
  private readonly plansBase = apiUrl('/api/succession-plans');
  private readonly criticalBase = apiUrl('/api/critical-positions');

  getPlansPaged(filter: SuccessionPlanFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      criticalPositionId: filter.criticalPositionId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<SuccessionPlanDto>>>(this.plansBase, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPlanById(id: string) {
    return this.http
      .get<ApiResponse<SuccessionPlanDto>>(`${this.plansBase}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getCriticalPositionsPaged(filter: CriticalPositionFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      positionId: filter.positionId ?? undefined,
      activeOnly: filter.activeOnly ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<CriticalPositionDto>>>(this.criticalBase, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  createPlan(body: CreateSuccessionPlanRequest) {
    return this.http
      .post<ApiResponse<SuccessionPlanDto>>(this.plansBase, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
