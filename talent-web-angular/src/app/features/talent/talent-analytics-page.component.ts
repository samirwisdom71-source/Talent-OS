import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import {
  TalentClassificationByCycleSummaryDto,
  TalentDistributionSummaryDto,
} from '../../shared/models/domain-analytics.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-talent-analytics-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, RouterLink],
  templateUrl: './talent-analytics-page.component.html',
  styleUrl: './talent-analytics-page.component.scss',
})
export class TalentAnalyticsPageComponent implements OnInit {
  private readonly api = inject(DomainAnalyticsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly cycles = signal<LookupItemDto[]>([]);
  readonly orgUnits = signal<LookupItemDto[]>([]);

  filterPerformanceCycleId = '';
  filterOrganizationUnitId = '';

  readonly distribution = signal<TalentDistributionSummaryDto | null>(null);
  readonly byCycle = signal<readonly TalentClassificationByCycleSummaryDto[]>([]);
  readonly busy = signal(false);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.cyclesApi.getLookup({ take: 200, lang: this.i18n.lang() }).subscribe({
      next: (r) => this.cycles.set(r),
      error: () => this.cycles.set([]),
    });
    this.identityLookups.getOrganizationUnits(undefined, 200).subscribe({
      next: (r) => this.orgUnits.set(r),
      error: () => this.orgUnits.set([]),
    });
    this.refresh();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  nineBoxLabel(code: number): string {
    return EnumLabels.nineBoxCode(this.lang(), code);
  }

  bandLabel(kind: 'perf' | 'pot', value: number): string {
    return kind === 'perf' ? EnumLabels.criticalityLevel(this.lang(), value) : EnumLabels.potentialLevel(this.lang(), value);
  }

  refresh(): void {
    this.failed.set(false);
    this.busy.set(true);
    const filter = {
      performanceCycleId: this.filterPerformanceCycleId || null,
      organizationUnitId: this.filterOrganizationUnitId || null,
    };
    forkJoin({
      distribution: this.api.getTalentDistribution(filter),
      byCycle: this.api.getTalentByCycle(filter),
    }).subscribe({
      next: ({ distribution, byCycle }) => {
        this.distribution.set(distribution);
        this.byCycle.set(byCycle);
        this.busy.set(false);
      },
      error: () => {
        this.distribution.set(null);
        this.byCycle.set([]);
        this.busy.set(false);
        this.failed.set(true);
      },
    });
  }

  cycleDisplayName(row: TalentClassificationByCycleSummaryDto): string {
    const fromLookup = this.cycles().find((c) => c.id === row.performanceCycleId)?.name;
    return fromLookup ?? row.performanceCycleNameEn;
  }

  canTalentAdmin(): boolean {
    return this.auth.hasAnyPermission([PermissionCodes.ClassificationView, PermissionCodes.ClassificationManage]);
  }
}
