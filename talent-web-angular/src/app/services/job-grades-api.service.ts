import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateJobGradeRequest,
  JobGradeDto,
  JobGradeFilterRequest,
  UpdateJobGradeRequest,
} from '../shared/models/job-grade.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class JobGradesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/job-grades');

  getPaged(filter: JobGradeFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      search: filter.search ?? undefined,
      level: filter.level ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<JobGradeDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<JobGradeDto>>(`${this.base}/${id}`).pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateJobGradeRequest) {
    return this.http.post<ApiResponse<JobGradeDto>>(this.base, body).pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateJobGradeRequest) {
    return this.http.put<ApiResponse<JobGradeDto>>(`${this.base}/${id}`, body).pipe(map((r) => unwrapApiResponse(r)));
  }
}
