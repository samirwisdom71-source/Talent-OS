import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateOrganizationUnitRequest,
  OrganizationUnitDto,
  OrganizationUnitFilterRequest,
  UpdateOrganizationUnitRequest,
} from '../shared/models/organization-unit.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class OrganizationUnitsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/organization-units');

  getPaged(filter: OrganizationUnitFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
      parentId: filter.parentId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<OrganizationUnitDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<OrganizationUnitDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateOrganizationUnitRequest) {
    return this.http.post<ApiResponse<OrganizationUnitDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateOrganizationUnitRequest) {
    return this.http.put<ApiResponse<OrganizationUnitDto>>(`${this.base}/${id}`, body).pipe(map((r) => unwrapApiResponse(r)));
  }
}
