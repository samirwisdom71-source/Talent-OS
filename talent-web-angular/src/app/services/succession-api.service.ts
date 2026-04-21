import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import { LookupItemDto } from '../shared/models/lookup.models';
import {
  CreateCriticalPositionRequest,
  CreateSuccessionPlanRequest,
  CriticalPositionDto,
  CriticalPositionFilterRequest,
  SuccessionPlanDto,
  SuccessionPlanFilterRequest,
  UpdateCriticalPositionRequest,
  UpdateSuccessionPlanRequest,
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

  getCriticalPositionById(id: string) {
    return this.http
      .get<ApiResponse<CriticalPositionDto>>(`${this.criticalBase}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  createCriticalPosition(body: CreateCriticalPositionRequest) {
    return this.http
      .post<ApiResponse<CriticalPositionDto>>(this.criticalBase, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  updateCriticalPosition(id: string, body: UpdateCriticalPositionRequest) {
    return this.http
      .put<ApiResponse<CriticalPositionDto>>(`${this.criticalBase}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  deactivateCriticalPosition(id: string) {
    return this.http
      .post<ApiResponse<CriticalPositionDto>>(`${this.criticalBase}/${id}/deactivate`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  /** GET /api/critical-positions/lookup — معرّف المنصب الحرج + اسم عرض من المنصب الوظيفي. */
  getCriticalPositionsLookup(filter: {
    search?: string | null;
    take?: number;
    lang?: 'ar' | 'en';
    activeOnly?: boolean;
  }) {
    const params = toHttpParams({
      search: filter.search ?? undefined,
      take: filter.take ?? undefined,
      lang: filter.lang ?? 'ar',
      activeOnly: filter.activeOnly ?? true,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.criticalBase}/lookup`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  /** GET /api/succession-plans/lookup — معرّف الخطة + اسم الخطة. */
  getSuccessionPlansLookup(filter: {
    search?: string | null;
    take?: number;
    criticalPositionId?: string | null;
    status?: number | null;
  }) {
    const params = toHttpParams({
      search: filter.search ?? undefined,
      take: filter.take ?? undefined,
      criticalPositionId: filter.criticalPositionId ?? undefined,
      status: filter.status ?? undefined,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.plansBase}/lookup`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  createPlan(body: CreateSuccessionPlanRequest) {
    return this.http
      .post<ApiResponse<SuccessionPlanDto>>(this.plansBase, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  updatePlan(id: string, body: UpdateSuccessionPlanRequest) {
    return this.http
      .put<ApiResponse<SuccessionPlanDto>>(`${this.plansBase}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  activatePlan(id: string) {
    return this.http
      .post<ApiResponse<SuccessionPlanDto>>(`${this.plansBase}/${id}/activate`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  closePlan(id: string) {
    return this.http
      .post<ApiResponse<SuccessionPlanDto>>(`${this.plansBase}/${id}/close`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
