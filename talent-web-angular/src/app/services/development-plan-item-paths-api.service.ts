import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import {
  CreateDevelopmentPlanItemPathRequest,
  DevelopmentPlanItemPathDto,
  UpdateDevelopmentPlanItemPathRequest,
} from '../shared/models/development.models';
import { unwrapApiResponse, unwrapApiVoid } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class DevelopmentPlanItemPathsApiService {
  private readonly http = inject(HttpClient);

  private base(planItemId: string) {
    return apiUrl(`/api/development-plan-items/${planItemId}/paths`);
  }

  add(planItemId: string, body: CreateDevelopmentPlanItemPathRequest) {
    return this.http
      .post<ApiResponse<DevelopmentPlanItemPathDto>>(this.base(planItemId), body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(planItemId: string, pathId: string, body: UpdateDevelopmentPlanItemPathRequest) {
    return this.http
      .put<ApiResponse<DevelopmentPlanItemPathDto>>(`${this.base(planItemId)}/${pathId}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  remove(planItemId: string, pathId: string) {
    return this.http
      .delete<ApiResponse>(`${this.base(planItemId)}/${pathId}`)
      .pipe(map((r) => unwrapApiVoid(r)));
  }

  markCompleted(planItemId: string, pathId: string) {
    return this.http
      .post<ApiResponse<DevelopmentPlanItemPathDto>>(`${this.base(planItemId)}/${pathId}/mark-completed`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
