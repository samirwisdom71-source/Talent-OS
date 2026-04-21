import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreatePerformanceCycleRequest,
  PerformanceCycleDto,
  PerformanceCycleFilterRequest,
  UpdatePerformanceCycleRequest,
} from '../shared/models/performance.models';
import { LookupItemDto } from '../shared/models/lookup.models';
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

  /** GET /api/performance-cycles/lookup — معرّف + اسم فقط (للقوائم المنسدلة). */
  getLookup(filter: {
    search?: string | null;
    status?: number | null;
    take?: number;
    lang?: 'ar' | 'en';
  }) {
    const params = toHttpParams({
      search: filter.search ?? undefined,
      status: filter.status ?? undefined,
      take: filter.take ?? 200,
      lang: filter.lang ?? 'ar',
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.base}/lookup`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<PerformanceCycleDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreatePerformanceCycleRequest) {
    return this.http.post<ApiResponse<PerformanceCycleDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdatePerformanceCycleRequest) {
    return this.http
      .put<ApiResponse<PerformanceCycleDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  activate(id: string) {
    return this.http
      .post<ApiResponse<PerformanceCycleDto>>(`${this.base}/${id}/activate`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  close(id: string) {
    return this.http
      .post<ApiResponse<PerformanceCycleDto>>(`${this.base}/${id}/close`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
