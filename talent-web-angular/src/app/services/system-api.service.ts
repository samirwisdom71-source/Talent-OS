import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse } from '../shared/models/api.types';
import { unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class SystemApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/system');

  health() {
    return this.http.get<ApiResponse<string>>(`${this.base}/health`).pipe(map((r) => unwrapApiResponse(r)));
  }

  downloadExcelTemplate(table: string, withSampleData = false) {
    return this.http.get(`${this.base}/excel-template`, {
      params: { table, withSampleData },
      responseType: 'blob',
    });
  }

  importExcel(file: File, table: string) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<ApiResponse<ExcelImportResponse>>(`${this.base}/excel-import`, formData, { params: { table } })
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}

export interface ExcelImportResponse {
  tables: ExcelImportTableResult[];
}

export interface ExcelImportTableResult {
  table: string;
  inserted: number;
  skipped: number;
  errors: string[];
}
