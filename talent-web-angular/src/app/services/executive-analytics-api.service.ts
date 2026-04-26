import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import { ExecutiveDashboardSummaryDto } from '../shared/models/analytics.models';
import { AnalyticsDateRangeQuery } from '../shared/models/domain-analytics.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class ExecutiveAnalyticsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/analytics/executive');

  getSummary(dateRange?: AnalyticsDateRangeQuery | null) {
    const params = toHttpParams({
      fromUtc: dateRange?.fromUtc ?? undefined,
      toUtc: dateRange?.toUtc ?? undefined,
    });
    return this.http
      .get<ApiResponse<ExecutiveDashboardSummaryDto>>(`${this.base}/summary`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
