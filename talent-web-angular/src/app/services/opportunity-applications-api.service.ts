import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  ApplyOpportunityRequest,
  OpportunityApplicationDto,
  OpportunityApplicationFilterRequest,
  UpdateOpportunityApplicationRequest,
} from '../shared/models/opportunity-application.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class OpportunityApplicationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/opportunity-applications');

  getPaged(filter: OpportunityApplicationFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      marketplaceOpportunityId: filter.marketplaceOpportunityId ?? undefined,
      employeeId: filter.employeeId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<OpportunityApplicationDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  apply(body: ApplyOpportunityRequest) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/apply`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateOpportunityApplicationRequest) {
    return this.http
      .put<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  withdraw(id: string) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}/withdraw`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  markUnderReview(id: string) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}/under-review`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  shortlist(id: string) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}/shortlist`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  accept(id: string) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}/accept`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  reject(id: string) {
    return this.http
      .post<ApiResponse<OpportunityApplicationDto>>(`${this.base}/${id}/reject`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
