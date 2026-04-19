import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import { PerformanceCycleDto, PerformanceCycleFilterRequest } from '../shared/models/performance.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class PerformanceCyclesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/performance-cycles');

  getPaged(filter: PerformanceCycleFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
      status: filter.status ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<PerformanceCycleDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<PerformanceCycleDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
