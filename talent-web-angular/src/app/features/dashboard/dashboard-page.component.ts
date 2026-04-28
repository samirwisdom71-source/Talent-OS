import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { ExecutiveAnalyticsApiService } from '../../services/executive-analytics-api.service';
import {
  DevelopmentAnalyticsSummaryDto,
  MarketplaceAnalyticsSummaryDto,
  PerformanceAnalyticsSummaryDto,
  SuccessionAnalyticsSummaryDto,
  TalentDistributionSummaryDto,
} from '../../shared/models/domain-analytics.models';
import { ExecutiveDashboardSummaryDto } from '../../shared/models/analytics.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { AuthService } from '../../core/auth/auth.service';
import { ChartWidgetComponent } from '../../shared/charts/chart-widget.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { AnalyticsDateRangeQuery } from '../../shared/models/domain-analytics.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import {
  performanceBandI18nKey,
  potentialBandI18nKey,
  readinessLevelI18nKey,
} from '../analytics/executive-analytics-labels';
import { opportunityTypeI18nKey } from '../analytics/performance-kpis-enum-labels';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [DecimalPipe, FormsModule, RouterLink, ChartWidgetComponent, EmptyStateComponent, TranslatePipe],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
})
export class DashboardPageComponent implements OnInit {
  private readonly analytics = inject(ExecutiveAnalyticsApiService);
  private readonly domain = inject(DomainAnalyticsApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly summary = signal<ExecutiveDashboardSummaryDto | null>(null);
  readonly talentDist = signal<TalentDistributionSummaryDto | null>(null);
  readonly succession = signal<SuccessionAnalyticsSummaryDto | null>(null);
  readonly development = signal<DevelopmentAnalyticsSummaryDto | null>(null);
  readonly marketplace = signal<MarketplaceAnalyticsSummaryDto | null>(null);
  readonly performance = signal<PerformanceAnalyticsSummaryDto | null>(null);
  readonly loadError = signal(false);
  readonly anim = signal<Record<string, number>>({});

  readonly chartTalentLabels = signal<string[]>([]);
  readonly chartTalentValues = signal<number[]>([]);
  readonly chartPerformanceLabels = signal<string[]>([]);
  readonly chartPerformanceValues = signal<number[]>([]);
  readonly chartSuccessionLabels = signal<string[]>([]);
  readonly chartSuccessionValues = signal<number[]>([]);
  readonly chartOpsLabels = signal<string[]>([]);
  readonly chartOpsValues = signal<number[]>([]);
  readonly chartExecutiveDetailsLabels = signal<string[]>([]);
  readonly chartExecutiveDetailsValues = signal<number[]>([]);
  readonly chartPerfBandLabels = signal<string[]>([]);
  readonly chartPerfBandValues = signal<number[]>([]);
  readonly chartPotentialBandLabels = signal<string[]>([]);
  readonly chartPotentialBandValues = signal<number[]>([]);
  readonly chartReadinessLabels = signal<string[]>([]);
  readonly chartReadinessValues = signal<number[]>([]);
  readonly chartMarketTypeLabels = signal<string[]>([]);
  readonly chartMarketTypeValues = signal<number[]>([]);
  readonly dateFrom = signal('');
  readonly dateTo = signal('');
  readonly colorsNine = ['#6366F1', '#06B6D4', '#14B8A6', '#A855F7', '#F59E0B', '#EC4899', '#22C55E', '#3B82F6', '#F97316'];
  readonly colorsOps = ['#4F46E5', '#0EA5E9', '#14B8A6', '#F59E0B'];
  readonly colorsPerformance = ['#A855F7'];
  readonly colorsSuccession = ['#6366F1', '#22C55E', '#F59E0B'];
  readonly colorsPerfBand = ['#0EA5E9'];
  readonly colorsMarketType = ['#8B5CF6', '#06B6D4', '#14B8A6', '#F59E0B', '#EC4899', '#3B82F6', '#22C55E', '#F97316'];
  readonly colorsPotentialBand = ['#EC4899'];
  readonly colorsReadiness = ['#10B981'];
  readonly colorsExecutiveDetails = ['#6366F1', '#8B5CF6', '#06B6D4', '#14B8A6', '#F59E0B', '#EC4899'];

  ngOnInit(): void {
    this.reload();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  nineLabel(code: number): string {
    return EnumLabels.nineBoxCode(this.lang(), code);
  }

  reload(): void {
    const dateRange = this.buildDateRange();
    if (this.dateFrom() && this.dateTo() && !dateRange) {
      this.loadError.set(true);
      return;
    }
    this.loadError.set(false);
    forkJoin({
      exec: this.analytics.getSummary(dateRange).pipe(catchError(() => of(null))),
      talent: this.domain
        .getTalentDistribution({ fromUtc: dateRange?.fromUtc ?? undefined, toUtc: dateRange?.toUtc ?? undefined })
        .pipe(catchError(() => of(null))),
      succ: this.domain.getSuccessionSummary(dateRange).pipe(catchError(() => of(null))),
      dev: this.domain.getDevelopmentSummary(dateRange).pipe(catchError(() => of(null))),
      mkt: this.domain.getMarketplaceSummary(dateRange).pipe(catchError(() => of(null))),
      perf: this.domain.getPerformanceSummary(dateRange).pipe(catchError(() => of(null))),
    }).subscribe({
      next: ({ exec, talent, succ, dev, mkt, perf }) => {
        this.summary.set(exec);
        this.talentDist.set(talent);
        this.succession.set(succ);
        this.development.set(dev);
        this.marketplace.set(mkt);
        this.performance.set(perf);
        if (!exec) {
          this.loadError.set(true);
          return;
        }
        this.loadError.set(false);
        this.animateExec(exec);
        if (talent?.byNineBox?.length) {
          this.chartTalentLabels.set(talent.byNineBox.map((x) => this.nineLabel(x.nineBoxCode)));
          this.chartTalentValues.set(talent.byNineBox.map((x) => x.count));
        } else {
          this.chartTalentLabels.set([]);
          this.chartTalentValues.set([]);
        }
        this.chartOpsLabels.set(['الدورات النشطة', 'خطط التعاقب', 'فرص مفتوحة', 'خطط تطوير نشطة']);
        this.chartOpsValues.set([
          exec.activePerformanceCycleCount,
          exec.activeSuccessionPlanCount,
          exec.openMarketplaceOpportunityCount,
          exec.activeDevelopmentPlanCount,
        ]);
        this.chartPerformanceLabels.set(['الأهداف', 'التقييمات']);
        this.chartPerformanceValues.set([
          perf?.totalGoals ?? exec.totalPerformanceGoals ?? 0,
          perf?.totalEvaluations ?? exec.finalizedEvaluationCount ?? 0,
        ]);
        this.chartSuccessionLabels.set(['خطط نشطة', 'بمرشح أساسي', 'جاهز الآن']);
        this.chartSuccessionValues.set([
          succ?.activeSuccessionPlans ?? exec.activeSuccessionPlanCount,
          succ?.plansWithPrimarySuccessor ?? 0,
          succ?.plansWithReadyNowSuccessor ?? 0,
        ]);
        this.chartExecutiveDetailsLabels.set([
          'تصنيفات',
          'درجات مواهب',
          'أداء مرتفع',
          'قادة استراتيجيون',
          'خطط تعاقب',
          'خطط تطوير',
        ]);
        this.chartExecutiveDetailsValues.set([
          exec.totalTalentClassifications,
          exec.totalTalentScores,
          exec.highPerformerCount,
          exec.strategicLeaderCount,
          exec.activeSuccessionPlanCount,
          exec.activeDevelopmentPlanCount,
        ]);
        const perfBand = [...(exec.byPerformanceBand ?? [])].sort((a, b) => a.value - b.value);
        this.chartPerfBandLabels.set(perfBand.map((x) => this.i18n.t(performanceBandI18nKey(x.value))));
        this.chartPerfBandValues.set(perfBand.map((x) => x.count));

        const potentialBand = [...(exec.byPotentialBand ?? [])].sort((a, b) => a.value - b.value);
        this.chartPotentialBandLabels.set(potentialBand.map((x) => this.i18n.t(potentialBandI18nKey(x.value))));
        this.chartPotentialBandValues.set(potentialBand.map((x) => x.count));

        const readiness = [...(exec.successorReadiness ?? [])].sort((a, b) => a.value - b.value);
        this.chartReadinessLabels.set(readiness.map((x) => this.i18n.t(readinessLevelI18nKey(x.value))));
        this.chartReadinessValues.set(readiness.map((x) => x.count));

        const marketType = [...(exec.marketplaceOpportunitiesByType ?? [])].sort((a, b) => a.code - b.code);
        this.chartMarketTypeLabels.set(marketType.map((x) => this.i18n.t(opportunityTypeI18nKey(x.code))));
        this.chartMarketTypeValues.set(marketType.map((x) => x.count));
      },
      error: () => {
        this.loadError.set(true);
        this.summary.set(null);
      },
    });
  }

  applyQuickRange(days: number): void {
    const today = new Date();
    const from = new Date(today);
    from.setDate(today.getDate() - days + 1);
    this.dateFrom.set(this.toDateInput(from));
    this.dateTo.set(this.toDateInput(today));
    this.reload();
  }

  clearDateFilter(): void {
    this.dateFrom.set('');
    this.dateTo.set('');
    this.reload();
  }

  private toDateInput(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private buildDateRange(): AnalyticsDateRangeQuery | null {
    const from = this.dateFrom();
    const to = this.dateTo();
    if (!from || !to) return null;
    const fromUtc = new Date(`${from}T00:00:00.000Z`);
    const toUtc = new Date(`${to}T23:59:59.999Z`);
    if (Number.isNaN(fromUtc.getTime()) || Number.isNaN(toUtc.getTime()) || fromUtc > toUtc) {
      return null;
    }
    return { fromUtc: fromUtc.toISOString(), toUtc: toUtc.toISOString() };
  }

  private animateExec(exec: ExecutiveDashboardSummaryDto): void {
    const keys: (keyof ExecutiveDashboardSummaryDto)[] = [
      'totalEmployees',
      'highPotentialCount',
      'activePerformanceCycleCount',
      'openMarketplaceOpportunityCount',
    ];
    for (const k of keys) {
      this.animateKey(k, Number(exec[k] ?? 0));
    }
  }

  private animateKey(key: string, end: number): void {
    const duration = 820;
    const t0 = performance.now();
    const tick = (now: number): void => {
      const p = Math.min(1, (now - t0) / duration);
      const eased = 1 - Math.pow(1 - p, 3);
      const v = Math.round(end * eased);
      this.anim.update((m) => ({ ...m, [key]: v }));
      if (p < 1) requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
  }

  display(key: keyof ExecutiveDashboardSummaryDto, raw: ExecutiveDashboardSummaryDto): number {
    const a = this.anim()[key as string];
    return a !== undefined ? a : Number(raw[key] ?? 0);
  }
}
