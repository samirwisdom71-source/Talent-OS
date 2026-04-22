import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin } from 'rxjs';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { DevelopmentAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { DevelopmentPlanDto } from '../../shared/models/development.models';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-development-list-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, FormsModule, TranslatePipe],
  templateUrl: './development-list-page.component.html',
  styleUrl: './development-list-page.component.scss',
})
export class DevelopmentListPageComponent implements OnInit {
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<DevelopmentPlanDto> | null>(null);
  readonly summary = signal<DevelopmentAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);
  readonly employees = signal<LookupItemDto[]>([]);
  readonly cycles = signal<LookupItemDto[]>([]);
  readonly busy = signal(false);

  page = 1;
  readonly pageSize = 20;
  employeeId = '';
  performanceCycleId = '';
  viewMode: ViewMode = 'table';

  ngOnInit(): void {
    this.loadLookups();
    this.load();
    this.loadSummary();
  }

  loadLookups(): void {
    this.lookups.getEmployees('', 200).subscribe({
      next: (items) => this.employees.set(items),
      error: () => this.employees.set([]),
    });
    this.cyclesApi.getLookup({ take: 200, lang: this.i18n.lang() }).subscribe({
      next: (items) => this.cycles.set(items),
      error: () => this.cycles.set([]),
    });
  }

  loadSummary(): void {
    this.analytics.getDevelopmentSummary().subscribe({
      next: (dev) => {
        this.summary.set(dev);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.summary.set(null);
        this.summaryFailed.set(true);
      },
    });
  }

  load(): void {
    this.busy.set(true);
    this.failed.set(false);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        employeeId: this.employeeId || null,
        performanceCycleId: this.performanceCycleId || null,
      })
      .subscribe({
        next: (plans) => {
          this.data.set(plans);
          this.busy.set(false);
          this.failed.set(false);
        },
        error: () => {
          this.data.set(null);
          this.busy.set(false);
          this.failed.set(true);
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  prevPage(): void {
    if (!this.data()?.hasPreviousPage) return;
    this.page -= 1;
    this.load();
  }

  nextPage(): void {
    if (!this.data()?.hasNextPage) return;
    this.page += 1;
    this.load();
  }

  employeeName(id: string): string {
    return this.employees().find((e) => e.id === id)?.name || id;
  }

  sourceType(v: number): string {
    return EnumLabels.developmentSourceType(this.lang(), v);
  }

  cycleName(id: string): string {
    return this.cycles().find((c) => c.id === id)?.name || id;
  }

  refreshAll(): void {
    forkJoin({
      plans: this.api.getPaged({
        page: this.page,
        pageSize: this.pageSize,
        employeeId: this.employeeId || null,
        performanceCycleId: this.performanceCycleId || null,
      }),
      dev: this.analytics.getDevelopmentSummary(),
    }).subscribe({
      next: ({ plans, dev }) => {
        this.data.set(plans);
        this.summary.set(dev);
        this.busy.set(false);
        this.failed.set(false);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.summary.set(null);
        this.busy.set(false);
        this.failed.set(true);
        this.summaryFailed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  planStatus(s: number): string {
    return EnumLabels.developmentPlanStatus(this.lang(), s);
  }
}
