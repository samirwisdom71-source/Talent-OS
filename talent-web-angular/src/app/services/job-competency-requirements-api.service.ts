import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateJobCompetencyRequirementRequest,
  JobCompetencyRequirementDto,
  JobCompetencyRequirementFilterRequest,
  UpdateJobCompetencyRequirementRequest,
} from '../shared/models/job-competency-requirement.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class JobCompetencyRequirementsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/job-competency-requirements');

  getPaged(filter: JobCompetencyRequirementFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      positionId: filter.positionId ?? undefined,
      competencyId: filter.competencyId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<JobCompetencyRequirementDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<JobCompetencyRequirementDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateJobCompetencyRequirementRequest) {
    return this.http
      .post<ApiResponse<JobCompetencyRequirementDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  update(id: string, body: UpdateJobCompetencyRequirementRequest) {
    return this.http
      .put<ApiResponse<JobCompetencyRequirementDto>>(`${this.base}/${id}`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
