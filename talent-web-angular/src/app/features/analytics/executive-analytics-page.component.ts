import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChartWidgetComponent, ChartWidgetKind } from '../../shared/charts/chart-widget.component';
import { ExecutiveAnalyticsApiService } from '../../services/executive-analytics-api.service';
import { ExecutiveDashboardSummaryDto } from '../../shared/models/analytics.models';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import {
  performanceBandI18nKey,
  potentialBandI18nKey,
  readinessLevelI18nKey,
} from './executive-analytics-labels';
import { developmentItemTypeI18nKey, opportunityTypeI18nKey } from './performance-kpis-enum-labels';
import { AnalyticsDateRangeBarComponent } from '../../shared/analytics/analytics-date-range-bar.component';
import { AnalyticsDateRangeQuery } from '../../shared/models/domain-analytics.models';

@Component({
  selector: 'app-executive-analytics-page',
  standalone: true,
  imports: [
    DecimalPipe,
    RouterLink,
    EmptyStateComponent,
    TranslatePipe,
    ChartWidgetComponent,
    AnalyticsDateRangeBarComponent,
  ],
  templateUrl: './executive-analytics-page.component.html',
  styleUrl: './executive-analytics-page.component.scss',
})
export class ExecutiveAnalyticsPageComponent implements OnInit {
  private readonly analytics = inject(ExecutiveAnalyticsApiService);
  private readonly i18n = inject(I18nService);

  readonly summary = signal<ExecutiveDashboardSummaryDto | null>(null);
  readonly failed = signal(false);
  /** Applied UTC range for API summaries, or null for all-time. */
  readonly dateRange = signal<AnalyticsDateRangeQuery | null>(null);

  /** Per-chart visualization mode (bar / pie / doughnut / line). */
  private readonly vizModes = signal<Record<string, ChartWidgetKind>>({});

  /** Per-chart defaults: line for ordered trends where useful; bar/pie as requested per chart. */
  private readonly defaultVizById: Record<string, ChartWidgetKind> = {
    nine: 'doughnut',
    pBand: 'line',
    potBand: 'bar',
    cycles: 'line',
    cat: 'bar',
    dev: 'pie',
    mkt: 'bar',
    ready: 'line',
  };

  ngOnInit(): void {
    this.load();
  }

  onDateRangeChange(range: AnalyticsDateRangeQuery | null): void {
    this.dateRange.set(range);
    this.load();
  }

  load(): void {
    this.failed.set(false);
    this.analytics.getSummary(this.dateRange()).subscribe({
      next: (s) => {
        this.summary.set(s);
        this.failed.set(false);
      },
      error: () => {
        this.summary.set(null);
        this.failed.set(true);
      },
    });
  }

  viz(id: string): ChartWidgetKind {
    const def = this.defaultVizById[id] ?? 'bar';
    return this.vizModes()[id] ?? def;
  }

  setViz(id: string, kind: ChartWidgetKind): void {
    this.vizModes.update((m) => ({ ...m, [id]: kind }));
  }

  /** Ordered metrics: line + bar + pie. Distributions: doughnut + bar + pie. Categories: bar + line + pie. */
  alternativesFor(id: string): readonly ChartWidgetKind[] {
    switch (id) {
      case 'nine':
        return ['doughnut', 'bar', 'pie'];
      case 'pBand':
      case 'ready':
        return ['line', 'bar', 'pie'];
      case 'potBand':
        return ['bar', 'line', 'pie'];
      case 'cycles':
        return ['line', 'bar'];
      case 'cat':
        return ['bar', 'line', 'pie'];
      case 'dev':
        return ['pie', 'bar', 'line'];
      case 'mkt':
        return ['bar', 'line', 'pie'];
      default:
        return ['bar', 'pie', 'doughnut'];
    }
  }

  readonly nineBoxChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.nineBoxDistribution ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.nineBoxCode - b.nineBoxCode);
    return {
      labels: sorted.map((r) => `${this.i18n.t('analytics.performanceKpi.nineBoxShort')} ${r.nineBoxCode}`),
      values: sorted.map((r) => r.count),
    };
  });

  readonly performanceBandChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.byPerformanceBand ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.value - b.value);
    return {
      labels: sorted.map((r) => this.i18n.t(performanceBandI18nKey(r.value))),
      values: sorted.map((r) => r.count),
    };
  });

  readonly potentialBandChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.byPotentialBand ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.value - b.value);
    return {
      labels: sorted.map((r) => this.i18n.t(potentialBandI18nKey(r.value))),
      values: sorted.map((r) => r.count),
    };
  });

  readonly cycleChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.performanceByCycle ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const useAr = this.i18n.lang() === 'ar';
    return {
      labels: rows.map((r) => {
        const ar = (r.performanceCycleNameAr ?? '').trim();
        if (useAr && ar.length > 0) return ar;
        return r.performanceCycleNameEn;
      }),
      values: rows.map((r) => r.finalizedEvaluations),
    };
  });

  readonly categoriesChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.topTalentCategories ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    return {
      labels: rows.map((r) => r.name),
      values: rows.map((r) => r.count),
    };
  });

  readonly developmentTypeChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.developmentItemsByType ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.code - b.code);
    return {
      labels: sorted.map((r) => this.i18n.t(developmentItemTypeI18nKey(r.code))),
      values: sorted.map((r) => r.count),
    };
  });

  readonly marketplaceTypeChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.marketplaceOpportunitiesByType ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.code - b.code);
    return {
      labels: sorted.map((r) => this.i18n.t(opportunityTypeI18nKey(r.code))),
      values: sorted.map((r) => r.count),
    };
  });

  readonly readinessChart = computed(() => {
    this.i18n.lang();
    const rows = this.summary()?.successorReadiness ?? [];
    if (!rows.length) return { labels: [] as string[], values: [] as number[] };
    const sorted = [...rows].sort((a, b) => a.value - b.value);
    return {
      labels: sorted.map((r) => this.i18n.t(readinessLevelI18nKey(r.value))),
      values: sorted.map((r) => r.count),
    };
  });

  hasExtendedAnalytics(s: ExecutiveDashboardSummaryDto): boolean {
    return (
      (s.nineBoxDistribution?.length ?? 0) > 0 ||
      (s.byPerformanceBand?.length ?? 0) > 0 ||
      (s.byPotentialBand?.length ?? 0) > 0 ||
      (s.performanceByCycle?.length ?? 0) > 0 ||
      (s.topTalentCategories?.length ?? 0) > 0 ||
      (s.developmentItemsByType?.length ?? 0) > 0 ||
      (s.marketplaceOpportunitiesByType?.length ?? 0) > 0 ||
      (s.successorReadiness?.length ?? 0) > 0
    );
  }
}
