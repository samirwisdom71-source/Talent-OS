import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  ClassificationRuleSetDto,
  ClassificationRuleSetFilterRequest,
  CreateClassificationRuleSetRequest,
  UpdateClassificationRuleSetRequest,
} from '../shared/models/classification-rule-set.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class ClassificationRuleSetsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/classification-rule-sets');

  getPaged(filter: ClassificationRuleSetFilterRequest) {
    const params = toHttpParams({
      page: filter.page ?? 1,
      pageSize: filter.pageSize ?? 20,
      search: filter.search ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<ClassificationRuleSetDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<ClassificationRuleSetDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateClassificationRuleSetRequest) {
    return this.http
      .post<ApiResponse<ClassificationRuleSetDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateClassificationRuleSetRequest) {
    return this.http
      .put<ApiResponse<ClassificationRuleSetDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  activate(id: string) {
    return this.http
      .post<ApiResponse<ClassificationRuleSetDto>>(`${this.base}/${id}/activate`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
