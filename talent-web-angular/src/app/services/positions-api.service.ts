import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import { CreatePositionRequest, PositionDto, PositionFilterRequest, UpdatePositionRequest } from '../shared/models/position.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class PositionsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/positions');

  getPaged(filter: PositionFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
      jobGradeId: filter.jobGradeId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<PositionDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<PositionDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreatePositionRequest) {
    return this.http.post<ApiResponse<PositionDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdatePositionRequest) {
    return this.http.put<ApiResponse<PositionDto>>(`${this.base}/${id}`, body).pipe(map((r) => unwrapApiResponse(r)));
  }
}
