import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CalculateTalentScoreRequest,
  RecalculateTalentScoreRequest,
  TalentScoreDto,
  TalentScoreFilterRequest,
} from '../shared/models/talent-score.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class TalentScoresApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/talent-scores');

  getPaged(filter: TalentScoreFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      minFinalScore: filter.minFinalScore ?? undefined,
      maxFinalScore: filter.maxFinalScore ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<TalentScoreDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<TalentScoreDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getByEmployeeCycle(employeeId: string, performanceCycleId: string) {
    const params = toHttpParams({ employeeId, performanceCycleId });
    return this.http
      .get<ApiResponse<TalentScoreDto>>(`${this.base}/by-employee-cycle`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  calculate(body: CalculateTalentScoreRequest) {
    return this.http
      .post<ApiResponse<TalentScoreDto>>(`${this.base}/calculate`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  recalculate(body: RecalculateTalentScoreRequest) {
    return this.http
      .post<ApiResponse<TalentScoreDto>>(`${this.base}/recalculate`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
