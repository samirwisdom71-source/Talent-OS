import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreatePotentialAssessmentRequest,
  PotentialAssessmentDto,
  PotentialAssessmentFilterRequest,
  UpdatePotentialAssessmentRequest,
} from '../shared/models/potential.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class PotentialAssessmentsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/potential-assessments');

  getPaged(filter: PotentialAssessmentFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      potentialLevel: filter.potentialLevel ?? undefined,
      status: filter.status ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<PotentialAssessmentDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<PotentialAssessmentDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreatePotentialAssessmentRequest) {
    return this.http
      .post<ApiResponse<PotentialAssessmentDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdatePotentialAssessmentRequest) {
    return this.http
      .put<ApiResponse<PotentialAssessmentDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
