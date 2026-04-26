import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { EmployeesApiService } from '../../services/employees-api.service';
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
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-development-list-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, FormsModule, TranslatePipe, LookupSearchComboComponent],
  templateUrl: './development-list-page.component.html',
  styleUrl: './development-list-page.component.scss',
})
export class DevelopmentListPageComponent implements OnInit {
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly employeesApi = inject(EmployeesApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<DevelopmentPlanDto> | null>(null);
  readonly summary = signal<DevelopmentAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);
  /** أسماء موظفين معروفة (لوكاب أولية + تعبئة من الصفحة) */
  readonly employeeNames = signal<Map<string, string>>(new Map());
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
    this.lookups.getEmployees('', 400).subscribe({
      next: (items) => this.mergeEmployeeNames(items.map((x) => [x.id, x.name] as const)),
      error: () => {},
    });
    this.cyclesApi.getLookup({ take: 200, lang: this.i18n.lang() }).subscribe({
      next: (items) => this.cycles.set(items),
      error: () => this.cycles.set([]),
    });
  }

  private mergeEmployeeNames(entries: ReadonlyArray<readonly [string, string]>): void {
    if (!entries.length) return;
    const m = new Map(this.employeeNames());
    for (const [id, name] of entries) {
      if (id && name) m.set(id, name);
    }
    this.employeeNames.set(m);
  }

  private hydrateEmployeeNamesFromPlans(plans: readonly DevelopmentPlanDto[]): void {
    const ids = [...new Set(plans.map((p) => p.employeeId).filter(Boolean))];
    const m = this.employeeNames();
    const missing = ids.filter((id) => !m.has(id));
    if (!missing.length) return;
    forkJoin(
      missing.map((id) => this.employeesApi.getById(id).pipe(catchError(() => of(null)))),
    ).subscribe((rows) => {
      const next = new Map(this.employeeNames());
      missing.forEach((id, i) => {
        const r = rows[i];
        if (r) {
          const label =
            this.i18n.lang() === 'ar'
              ? r.fullNameAr?.trim() || r.fullNameEn?.trim()
              : r.fullNameEn?.trim() || r.fullNameAr?.trim();
          if (label) next.set(id, label);
        }
      });
      this.employeeNames.set(next);
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
          this.hydrateEmployeeNamesFromPlans(plans.items);
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
    return this.employeeNames().get(id) || id;
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
        this.hydrateEmployeeNamesFromPlans(plans.items);
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
