import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CompetencyDto,
  CompetencyFilterRequest,
  CreateCompetencyRequest,
  UpdateCompetencyRequest,
} from '../shared/models/competency.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class CompetenciesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/competencies');

  getPaged(filter: CompetencyFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<CompetencyDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<CompetencyDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateCompetencyRequest) {
    return this.http.post<ApiResponse<CompetencyDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateCompetencyRequest) {
    return this.http.put<ApiResponse<CompetencyDto>>(`${this.base}/${id}`, body).pipe(map((r) => unwrapApiResponse(r)));
  }
}
