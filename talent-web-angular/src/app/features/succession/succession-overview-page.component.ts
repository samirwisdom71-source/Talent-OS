import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin } from 'rxjs';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { SuccessionAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { CriticalPositionDto, SuccessionPlanDto } from '../../shared/models/succession.models';
import { PermissionCodes as PermissionCodesConst } from '../../shared/models/permission-codes';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-succession-overview-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, TranslatePipe],
  templateUrl: './succession-overview-page.component.html',
  styleUrl: './succession-overview-page.component.scss',
})
export class SuccessionOverviewPageComponent implements OnInit {
  private readonly api = inject(SuccessionApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodesConst;

  readonly plans = signal<readonly SuccessionPlanDto[]>([]);
  readonly positions = signal<readonly CriticalPositionDto[]>([]);
  readonly summary = signal<SuccessionAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);

  private readonly positionLookup = signal(new Map<string, string>());

  ngOnInit(): void {
    forkJoin({
      plans: this.api.getPlansPaged({ page: 1, pageSize: 25 }),
      positions: this.api.getCriticalPositionsPaged({ page: 1, pageSize: 25, activeOnly: true }),
      sum: this.analytics.getSuccessionSummary(),
      posLookup: this.identityLookups.getPositions(undefined, 400),
    }).subscribe({
      next: ({ plans, positions, sum, posLookup }) => {
        this.plans.set(plans.items);
        this.positions.set(positions.items);
        this.summary.set(sum);
        this.positionLookup.set(new Map(posLookup.map((x) => [x.id, x.name])));
        this.failed.set(false);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.plans.set([]);
        this.positions.set([]);
        this.summary.set(null);
        this.positionLookup.set(new Map());
        this.failed.set(true);
        this.summaryFailed.set(true);
      },
    });
  }

  positionLabel(positionId: string): string {
    return this.positionLookup().get(positionId) ?? '';
  }

  canSuccessionSection(): boolean {
    return this.auth.hasAnyPermission([PermissionCodesConst.SuccessionView, PermissionCodesConst.SuccessionManage]);
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  planStatus(s: number): string {
    return EnumLabels.successionPlanStatus(this.lang(), s);
  }

  criticality(v: number): string {
    return EnumLabels.criticalityLevel(this.lang(), v);
  }

  risk(v: number): string {
    return EnumLabels.successionRiskLevel(this.lang(), v);
  }

  readinessCount(items: readonly { value: number; count: number }[], value: number): number {
    return items.find((x) => x.value === value)?.count ?? 0;
  }
}
