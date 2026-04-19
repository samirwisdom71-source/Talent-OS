import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateSuccessorCandidateRequest,
  SuccessorCandidateDto,
  SuccessorCandidateFilterRequest,
  UpdateSuccessorCandidateRequest,
} from '../shared/models/successor-candidate.models';
import { toHttpParams, unwrapApiResponse, unwrapApiVoid } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class SuccessorCandidatesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/successor-candidates');

  create(body: CreateSuccessorCandidateRequest) {
    return this.http
      .post<ApiResponse<SuccessorCandidateDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateSuccessorCandidateRequest) {
    return this.http
      .put<ApiResponse<SuccessorCandidateDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPaged(filter: SuccessorCandidateFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      successionPlanId: filter.successionPlanId,
    });
    return this.http
      .get<ApiResponse<PagedResult<SuccessorCandidateDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<SuccessorCandidateDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  markPrimary(id: string) {
    return this.http
      .post<ApiResponse<SuccessorCandidateDto>>(`${this.base}/${id}/mark-primary`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  remove(id: string) {
    return this.http
      .delete<ApiResponse>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiVoid(r)));
  }
}
