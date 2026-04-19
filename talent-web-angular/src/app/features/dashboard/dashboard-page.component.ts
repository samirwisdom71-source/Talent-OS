import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
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

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [DecimalPipe, RouterLink, ChartWidgetComponent, EmptyStateComponent],
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
  readonly chartOpsLabels = signal<string[]>([]);
  readonly chartOpsValues = signal<number[]>([]);

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
    this.loadError.set(false);
    forkJoin({
      exec: this.analytics.getSummary().pipe(catchError(() => of(null))),
      talent: this.domain.getTalentDistribution({}).pipe(catchError(() => of(null))),
      succ: this.domain.getSuccessionSummary().pipe(catchError(() => of(null))),
      dev: this.domain.getDevelopmentSummary().pipe(catchError(() => of(null))),
      mkt: this.domain.getMarketplaceSummary().pipe(catchError(() => of(null))),
      perf: this.domain.getPerformanceSummary().pipe(catchError(() => of(null))),
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
      },
      error: () => {
        this.loadError.set(true);
        this.summary.set(null);
      },
    });
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
