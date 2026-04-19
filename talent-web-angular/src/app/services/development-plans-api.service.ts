import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateDevelopmentPlanRequest,
  DevelopmentPlanDto,
  DevelopmentPlanFilterRequest,
} from '../shared/models/development.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class DevelopmentPlansApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/development-plans');

  getPaged(filter: DevelopmentPlanFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<DevelopmentPlanDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<DevelopmentPlanDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateDevelopmentPlanRequest) {
    return this.http
      .post<ApiResponse<DevelopmentPlanDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
