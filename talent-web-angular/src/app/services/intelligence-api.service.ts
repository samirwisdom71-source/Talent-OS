import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  GenerateCycleIntelligenceRequest,
  GenerateEmployeeIntelligenceRequest,
  IntelligenceGenerationResultDto,
  TalentInsightDto,
  TalentInsightFilterRequest,
  TalentRecommendationDto,
  TalentRecommendationFilterRequest,
} from '../shared/models/intelligence.models';
import { toHttpParams, unwrapApiResponse, unwrapApiVoid } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class IntelligenceApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/intelligence');

  getInsightsPaged(filter: TalentInsightFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      status: filter.status ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<TalentInsightDto>>>(`${this.base}/insights`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getInsightById(id: string) {
    return this.http
      .get<ApiResponse<TalentInsightDto>>(`${this.base}/insights/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getRecommendationsPaged(filter: TalentRecommendationFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      status: filter.status ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<TalentRecommendationDto>>>(`${this.base}/recommendations`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getRecommendationById(id: string) {
    return this.http
      .get<ApiResponse<TalentRecommendationDto>>(`${this.base}/recommendations/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  generateForEmployee(body: GenerateEmployeeIntelligenceRequest) {
    return this.http
      .post<ApiResponse<IntelligenceGenerationResultDto>>(`${this.base}/generate/employee`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  generateForCycle(body: GenerateCycleIntelligenceRequest) {
    return this.http
      .post<ApiResponse<IntelligenceGenerationResultDto>>(`${this.base}/generate/cycle`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  dismissInsight(id: string) {
    return this.http
      .post<ApiResponse>(`${this.base}/insights/${id}/dismiss`, {})
      .pipe(map((r) => unwrapApiVoid(r)));
  }

  dismissRecommendation(id: string) {
    return this.http
      .post<ApiResponse>(`${this.base}/recommendations/${id}/dismiss`, {})
      .pipe(map((r) => unwrapApiVoid(r)));
  }

  acceptRecommendation(id: string) {
    return this.http
      .post<ApiResponse>(`${this.base}/recommendations/${id}/accept`, {})
      .pipe(map((r) => unwrapApiVoid(r)));
  }
}
