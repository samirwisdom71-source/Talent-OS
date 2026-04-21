import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { LookupItemDto } from '../shared/models/lookup.models';
import { I18nService } from '../shared/services/i18n.service';
import { PerformanceCyclesApiService } from './performance-cycles-api.service';

export interface PerformanceCyclesLookupOptions {
  /** @deprecated لا يُستخدم مع مسار lookup؛ احتفظنا به للتوافق مع استدعاءات قديمة. */
  page?: number;
  /** يُمرَّر كـ `take` لـ GET /api/performance-cycles/lookup */
  pageSize?: number;
  search?: string | null;
  status?: number | null;
}

/**
 * يحمّل دورات الأداء للقوائم المنسدلة عبر **GET /api/performance-cycles/lookup**
 * (معرّف + اسم فقط، مع تطبيق صلاحيات الـ API على هذا المسار).
 */
@Injectable({ providedIn: 'root' })
export class PerformanceCyclesLookupService {
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly i18n = inject(I18nService);

  loadLookupItems(options: PerformanceCyclesLookupOptions = {}): Observable<LookupItemDto[]> {
    const take = options.pageSize ?? 200;
    return this.cyclesApi.getLookup({
      take,
      search: options.search ?? null,
      status: options.status ?? null,
      lang: this.i18n.lang(),
    });
  }
}
