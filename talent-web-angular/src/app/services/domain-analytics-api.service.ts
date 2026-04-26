import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import {
  DevelopmentAnalyticsSummaryDto,
  MarketplaceAnalyticsSummaryDto,
  PerformanceImpactFilterRequest,
  PerformanceImpactSummaryDto,
  PerformanceAnalyticsSummaryDto,
  SuccessionAnalyticsSummaryDto,
  AnalyticsDateRangeQuery,
  TalentAnalyticsFilterRequest,
  TalentClassificationByCycleSummaryDto,
  TalentDistributionSummaryDto,
} from '../shared/models/domain-analytics.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

function withDateRange(
  params: ReturnType<typeof toHttpParams>,
  dateRange?: AnalyticsDateRangeQuery | null,
): ReturnType<typeof toHttpParams> {
  if (!dateRange?.fromUtc || !dateRange?.toUtc) {
    return params;
  }
  return params.set('fromUtc', dateRange.fromUtc).set('toUtc', dateRange.toUtc);
}

@Injectable({ providedIn: 'root' })
export class DomainAnalyticsApiService {
  private readonly http = inject(HttpClient);

  getTalentDistribution(filter: TalentAnalyticsFilterRequest) {
    const params = toHttpParams({
      performanceCycleId: filter.performanceCycleId ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
      fromUtc: filter.fromUtc ?? undefined,
      toUtc: filter.toUtc ?? undefined,
    });
    return this.http
      .get<ApiResponse<TalentDistributionSummaryDto>>(apiUrl('/api/analytics/talent/distribution'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getTalentByCycle(filter: TalentAnalyticsFilterRequest) {
    const params = toHttpParams({
      performanceCycleId: filter.performanceCycleId ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
      fromUtc: filter.fromUtc ?? undefined,
      toUtc: filter.toUtc ?? undefined,
    });
    return this.http
      .get<ApiResponse<TalentClassificationByCycleSummaryDto[]>>(apiUrl('/api/analytics/talent/by-cycle'), {
        params,
      })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getSuccessionSummary(dateRange?: AnalyticsDateRangeQuery | null) {
    const params = withDateRange(new HttpParams(), dateRange);
    return this.http
      .get<ApiResponse<SuccessionAnalyticsSummaryDto>>(apiUrl('/api/analytics/succession/summary'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getDevelopmentSummary(dateRange?: AnalyticsDateRangeQuery | null) {
    const params = withDateRange(new HttpParams(), dateRange);
    return this.http
      .get<ApiResponse<DevelopmentAnalyticsSummaryDto>>(apiUrl('/api/analytics/development/summary'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getMarketplaceSummary(dateRange?: AnalyticsDateRangeQuery | null) {
    const params = withDateRange(new HttpParams(), dateRange);
    return this.http
      .get<ApiResponse<MarketplaceAnalyticsSummaryDto>>(apiUrl('/api/analytics/marketplace/summary'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPerformanceSummary(dateRange?: AnalyticsDateRangeQuery | null) {
    const params = withDateRange(new HttpParams(), dateRange);
    return this.http
      .get<ApiResponse<PerformanceAnalyticsSummaryDto>>(apiUrl('/api/analytics/performance/summary'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getPerformanceImpact(filter: PerformanceImpactFilterRequest) {
    const params = toHttpParams({
      beforeFromUtc: filter.beforeFromUtc ?? undefined,
      beforeToUtc: filter.beforeToUtc ?? undefined,
      afterFromUtc: filter.afterFromUtc ?? undefined,
      afterToUtc: filter.afterToUtc ?? undefined,
    });

    return this.http
      .get<ApiResponse<PerformanceImpactSummaryDto>>(apiUrl('/api/analytics/performance/impact'), { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
