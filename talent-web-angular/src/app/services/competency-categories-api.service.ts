import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CompetencyCategoryDto,
  CompetencyCategoryFilterRequest,
  CreateCompetencyCategoryRequest,
  UpdateCompetencyCategoryRequest,
} from '../shared/models/competency-category.models';
import { LookupItemDto } from '../shared/models/lookup.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class CompetencyCategoriesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/competency-categories');

  getPaged(filter: CompetencyCategoryFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<CompetencyCategoryDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<CompetencyCategoryDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateCompetencyCategoryRequest) {
    return this.http.post<ApiResponse<CompetencyCategoryDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateCompetencyCategoryRequest) {
    return this.http
      .put<ApiResponse<CompetencyCategoryDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  /** id + name (Arabic preferred) for dropdowns */
  getLookup(search?: string | null, take?: number | null) {
    const params = toHttpParams({
      search: search ?? undefined,
      take: take ?? undefined,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.base}/lookup`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
