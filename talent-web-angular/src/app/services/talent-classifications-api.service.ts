import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  ClassifyTalentClassificationRequest,
  ReclassifyTalentClassificationRequest,
  TalentClassificationDto,
  TalentClassificationFilterRequest,
} from '../shared/models/classification.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class TalentClassificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/talent-classifications');

  getPaged(filter: TalentClassificationFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      employeeId: filter.employeeId ?? undefined,
      performanceCycleId: filter.performanceCycleId ?? undefined,
      nineBoxCode: filter.nineBoxCode ?? undefined,
      isHighPotential: filter.isHighPotential ?? undefined,
      isHighPerformer: filter.isHighPerformer ?? undefined,
      performanceBand: filter.performanceBand ?? undefined,
      potentialBand: filter.potentialBand ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<TalentClassificationDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<TalentClassificationDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getByEmployeeCycle(employeeId: string, performanceCycleId: string) {
    const params = toHttpParams({ employeeId, performanceCycleId });
    return this.http
      .get<ApiResponse<TalentClassificationDto>>(`${this.base}/by-employee-cycle`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  classify(body: ClassifyTalentClassificationRequest) {
    return this.http
      .post<ApiResponse<TalentClassificationDto>>(`${this.base}/classify`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  reclassify(body: ReclassifyTalentClassificationRequest) {
    return this.http
      .post<ApiResponse<TalentClassificationDto>>(`${this.base}/reclassify`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
