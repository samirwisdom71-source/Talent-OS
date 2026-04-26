import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { catchError, forkJoin, of } from 'rxjs';
import { ChartWidgetComponent } from '../../shared/charts/chart-widget.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import {
  AnalyticsDateRangeQuery,
  DevelopmentAnalyticsSummaryDto,
  MarketplaceAnalyticsSummaryDto,
  PerformanceAnalyticsSummaryDto,
  SuccessionAnalyticsSummaryDto,
  TalentDistributionSummaryDto,
} from '../../shared/models/domain-analytics.models';
import { AnalyticsDateRangeBarComponent } from '../../shared/analytics/analytics-date-range-bar.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { developmentItemTypeI18nKey, opportunityTypeI18nKey } from './performance-kpis-enum-labels';

export interface PerformanceKpisBundle {
  readonly performance: PerformanceAnalyticsSummaryDto | null;
  readonly succession: SuccessionAnalyticsSummaryDto | null;
  readonly development: DevelopmentAnalyticsSummaryDto | null;
  readonly marketplace: MarketplaceAnalyticsSummaryDto | null;
  readonly talent: TalentDistributionSummaryDto | null;
}

@Component({
  selector: 'app-performance-kpis-page',
  standalone: true,
  imports: [DecimalPipe, TranslatePipe, ChartWidgetComponent, EmptyStateComponent, AnalyticsDateRangeBarComponent],
  templateUrl: './performance-kpis-page.component.html',
  styleUrl: './performance-kpis-page.component.scss',
})
export class PerformanceKpisPageComponent implements OnInit {
  private readonly api = inject(DomainAnalyticsApiService);
  private readonly i18n = inject(I18nService);

  readonly bundle = signal<PerformanceKpisBundle | null>(null);
  readonly loading = signal(false);
  readonly failed = signal(false);
  readonly dateRange = signal<AnalyticsDateRangeQuery | null>(null);

  readonly cycleChart = computed(() => {
    this.i18n.lang();
    const data = this.bundle()?.performance?.breakdownByCycle ?? [];
    const useAr = this.i18n.lang() === 'ar';
    return {
      labels: data.map((x) => {
        const ar = (x.performanceCycleNameAr ?? '').trim();
        if (useAr && ar.length > 0) return ar;
        return x.performanceCycleNameEn;
      }),
      values: data.map((x) => x.finalizedEvaluations),
    };
  });

  readonly talentNineBoxChart = computed(() => {
    this.i18n.lang();
    const rows = this.bundle()?.talent?.byNineBox ?? [];
    if (!rows.length) return null;
    const sorted = [...rows].sort((a, b) => a.nineBoxCode - b.nineBoxCode);
    return {
      labels: sorted.map(
        (x) => `${this.i18n.t('analytics.performanceKpi.nineBoxShort')} ${x.nineBoxCode}`,
      ),
      values: sorted.map((x) => x.count),
    };
  });

  readonly developmentByTypeChart = computed(() => {
    this.i18n.lang();
    const rows = this.bundle()?.development?.itemsByType ?? [];
    if (!rows.length) return null;
    return {
      labels: rows.map((x) => this.i18n.t(developmentItemTypeI18nKey(x.itemType))),
      values: rows.map((x) => x.itemCount),
    };
  });

  readonly marketplaceByTypeChart = computed(() => {
    this.i18n.lang();
    const rows = this.bundle()?.marketplace?.opportunitiesByType ?? [];
    if (!rows.length) return null;
    return {
      labels: rows.map((x) => this.i18n.t(opportunityTypeI18nKey(x.opportunityType))),
      values: rows.map((x) => x.opportunityCount),
    };
  });

  ngOnInit(): void {
    this.refresh();
  }

  onDateRangeChange(range: AnalyticsDateRangeQuery | null): void {
    this.dateRange.set(range);
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.failed.set(false);
    const d = this.dateRange();
    const talentFilter = {
      fromUtc: d?.fromUtc ?? undefined,
      toUtc: d?.toUtc ?? undefined,
    };
    forkJoin({
      performance: this.api.getPerformanceSummary(d).pipe(catchError(() => of(null))),
      succession: this.api.getSuccessionSummary(d).pipe(catchError(() => of(null))),
      development: this.api.getDevelopmentSummary(d).pipe(catchError(() => of(null))),
      marketplace: this.api.getMarketplaceSummary(d).pipe(catchError(() => of(null))),
      talent: this.api.getTalentDistribution(talentFilter).pipe(catchError(() => of(null))),
    }).subscribe({
      next: (data) => {
        this.bundle.set(data);
        this.loading.set(false);
        const empty =
          !data.performance &&
          !data.succession &&
          !data.development &&
          !data.marketplace &&
          !data.talent;
        if (empty) {
          this.failed.set(true);
        }
      },
      error: () => {
        this.bundle.set(null);
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }
}
