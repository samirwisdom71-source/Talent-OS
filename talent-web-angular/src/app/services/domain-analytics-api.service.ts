import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import {
  DevelopmentAnalyticsSummaryDto,
  MarketplaceAnalyticsSummaryDto,
  PerformanceAnalyticsSummaryDto,
  SuccessionAnalyticsSummaryDto,
  TalentAnalyticsFilterRequest,
  TalentClassificationByCycleSummaryDto,
  TalentDistributionSummaryDto,
} from '../shared/models/domain-analytics.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class DomainAnalyticsApiService {
  private readonly http = inject(HttpClient);

  getTalentDistribution(filter: TalentAnalyticsFilterRequest) {
    const params = toHttpParams({
      performanceCycleId: filter.performanceCycleId ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
    });
    return this.http
      .get<ApiResponse<TalentDistributionSummaryDto>>(apiUrl('/api/analytics/talent/distribution'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getTalentByCycle(filter: TalentAnalyticsFilterRequest) {
    const params = toHttpParams({
      performanceCycleId: filter.performanceCycleId ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
    });
    return this.http
      .get<ApiResponse<TalentClassificationByCycleSummaryDto[]>>(apiUrl('/api/analytics/talent/by-cycle'), {
        params,
      })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getSuccessionSummary() {
    return this.http
      .get<ApiResponse<SuccessionAnalyticsSummaryDto>>(apiUrl('/api/analytics/succession/summary'))
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getDevelopmentSummary() {
    return this.http
      .get<ApiResponse<DevelopmentAnalyticsSummaryDto>>(apiUrl('/api/analytics/development/summary'))
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getMarketplaceSummary() {
    return this.http
      .get<ApiResponse<MarketplaceAnalyticsSummaryDto>>(apiUrl('/api/analytics/marketplace/summary'))
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPerformanceSummary() {
    return this.http
      .get<ApiResponse<PerformanceAnalyticsSummaryDto>>(apiUrl('/api/analytics/performance/summary'))
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
