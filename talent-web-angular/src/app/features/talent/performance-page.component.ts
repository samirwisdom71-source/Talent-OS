import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { PerformanceAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-performance-page',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, TranslatePipe],
  templateUrl: './performance-page.component.html',
  styleUrl: './performance-page.component.scss',
})
export class PerformancePageComponent implements OnInit {
  private readonly api = inject(PerformanceCyclesApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  readonly i18n = inject(I18nService);

  readonly data = signal<PagedResult<PerformanceCycleDto> | null>(null);
  readonly summary = signal<PerformanceAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);

  ngOnInit(): void {
    forkJoin({
      cycles: this.api.getPaged({ page: 1, pageSize: 50 }),
      perf: this.analytics.getPerformanceSummary(),
    }).subscribe({
      next: ({ cycles, perf }) => {
        this.data.set(cycles);
        this.summary.set(perf);
        this.failed.set(false);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.summary.set(null);
        this.failed.set(true);
        this.summaryFailed.set(true);
      },
    });
  }

  cycleName(c: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? c.nameAr || c.nameEn : c.nameEn || c.nameAr;
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  cycleStatusLabel(s: number): string {
    return EnumLabels.performanceCycleStatus(this.lang(), s);
  }

  completionRate(): number {
    const s = this.summary();
    if (!s || s.totalGoals === 0) return 0;
    return Math.round((s.completedGoals / s.totalGoals) * 100);
  }
}
