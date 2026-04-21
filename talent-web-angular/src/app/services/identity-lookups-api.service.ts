import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import { LookupItemDto } from '../shared/models/lookup.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class IdentityLookupsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/identity/lookups');

  getEmployees(search?: string, take?: number) {
    return this.get('/employees', search, take);
  }

  getRoles(search?: string, take?: number) {
    return this.get('/roles', search, take);
  }

  getPositions(search?: string, take?: number) {
    return this.get('/positions', search, take);
  }

  getOrganizationUnits(search?: string, take?: number) {
    return this.get('/organization-units', search, take);
  }

  getJobGrades(search?: string, take?: number) {
    return this.get('/job-grades', search, take);
  }

  getCompetencies(search?: string, take?: number) {
    return this.get('/competencies', search, take);
  }

  getCompetencyLevels(search?: string, take?: number) {
    return this.get('/competency-levels', search, take);
  }

  /** @deprecated استخدم `PerformanceCyclesLookupService.loadLookupItems`. */
  getPerformanceCycles(search?: string, take = 100) {
    const params = toHttpParams({
      take,
      search: search ?? undefined,
      lang: 'ar',
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(apiUrl('/api/performance-cycles/lookup'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  private get(path: string, search?: string, take?: number) {
    const params = toHttpParams({
      search: search ?? undefined,
      take: take ?? undefined,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.base}${path}`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
