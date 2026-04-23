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

  getUsers(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/users', search, take, lang);
  }

  getRoles(search?: string, take?: number) {
    return this.get('/roles', search, take);
  }

  getPositions(search?: string, take?: number, organizationUnitId?: string) {
    const params = toHttpParams({
      search: search ?? undefined,
      take: take ?? undefined,
      organizationUnitId: organizationUnitId ?? undefined,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.base}/positions`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
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

  getPerformanceEvaluations(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/performance-evaluations', search, take, lang);
  }

  getTalentClassifications(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/talent-classifications', search, take, lang);
  }

  getDevelopmentPlans(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/development-plans', search, take, lang);
  }

  getMarketplaceOpportunities(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/marketplace-opportunities', search, take, lang);
  }

  getOpportunityApplications(search?: string, take?: number, lang?: 'ar' | 'en') {
    return this.get('/opportunity-applications', search, take, lang);
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

  private get(path: string, search?: string, take?: number, lang?: 'ar' | 'en') {
    const params = toHttpParams({
      search: search ?? undefined,
      take: take ?? undefined,
      lang: lang ?? undefined,
    });
    return this.http
      .get<ApiResponse<LookupItemDto[]>>(`${this.base}${path}`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
