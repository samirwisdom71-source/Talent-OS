import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CompetencyLevelDto,
  CompetencyLevelFilterRequest,
  CreateCompetencyLevelRequest,
  UpdateCompetencyLevelRequest,
} from '../shared/models/competency-level.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class CompetencyLevelsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/competency-levels');

  getPaged(filter: CompetencyLevelFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<CompetencyLevelDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<CompetencyLevelDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateCompetencyLevelRequest) {
    return this.http.post<ApiResponse<CompetencyLevelDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateCompetencyLevelRequest) {
    return this.http
      .put<ApiResponse<CompetencyLevelDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
