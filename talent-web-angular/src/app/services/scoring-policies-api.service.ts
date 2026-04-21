import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateScoringPolicyRequest,
  ScoringPolicyDto,
  ScoringPolicyFilterRequest,
  UpdateScoringPolicyRequest,
} from '../shared/models/scoring-policy.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class ScoringPoliciesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/scoring-policies');

  create(body: CreateScoringPolicyRequest) {
    return this.http
      .post<ApiResponse<ScoringPolicyDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateScoringPolicyRequest) {
    return this.http
      .put<ApiResponse<ScoringPolicyDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<ScoringPolicyDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPaged(filter: ScoringPolicyFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<ScoringPolicyDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  activate(id: string) {
    return this.http
      .post<ApiResponse<ScoringPolicyDto>>(`${this.base}/${id}/activate`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}

