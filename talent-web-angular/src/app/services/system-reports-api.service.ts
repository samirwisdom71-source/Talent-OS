import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import { SystemReportDto, SystemReportFilterRequest } from '../shared/models/system-report.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class SystemReportsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/system-reports');

  getReport(filter: SystemReportFilterRequest) {
    const params = toHttpParams({
      fromUtc: filter.fromUtc ?? undefined,
      toUtc: filter.toUtc ?? undefined,
      chartMonths: filter.chartMonths ?? undefined,
      language: filter.language ?? undefined,
    });

    return this.http
      .get<ApiResponse<SystemReportDto>>(this.base, { params })
      .pipe(map((response) => unwrapApiResponse(response)));
  }

  exportPdf(filter: SystemReportFilterRequest) {
    const params = toHttpParams({
      fromUtc: filter.fromUtc ?? undefined,
      toUtc: filter.toUtc ?? undefined,
      chartMonths: filter.chartMonths ?? undefined,
      language: filter.language ?? undefined,
    });

    return this.http.get(`${this.base}/export/pdf`, { params, responseType: 'blob' });
  }

  exportExcel(filter: SystemReportFilterRequest) {
    const params = toHttpParams({
      fromUtc: filter.fromUtc ?? undefined,
      toUtc: filter.toUtc ?? undefined,
      chartMonths: filter.chartMonths ?? undefined,
      language: filter.language ?? undefined,
    });

    return this.http.get(`${this.base}/export/excel`, { params, responseType: 'blob' });
  }
}
