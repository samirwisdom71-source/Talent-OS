import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { TalentScoresApiService } from '../../services/talent-scores-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CalculateTalentScoreRequest,
  RecalculateTalentScoreRequest,
  TalentScoreDto,
  TalentScoreFilterRequest,
} from '../../shared/models/talent-score.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-talent-scores-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, DatePipe, DecimalPipe, LookupSearchComboComponent],
  templateUrl: './talent-scores-page.component.html',
  styleUrl: './talent-scores-page.component.scss',
})
export class TalentScoresPageComponent implements OnInit {
  private readonly api = inject(TalentScoresApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;

  readonly cycles = signal<LookupItemDto[]>([]);
  readonly employees = signal<LookupItemDto[]>([]);

  filterEmployeeId = '';
  filterPerformanceCycleId = '';
  filterMinFinal: number | null = null;
  filterMaxFinal: number | null = null;

  page = 1;
  readonly pageSize = 20;

  readonly result = signal<PagedResult<TalentScoreDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly viewMode = signal<ViewMode>('table');

  readonly detailsOpen = signal(false);
  readonly calculateOpen = signal(false);
  readonly recalculateOpen = signal(false);
  readonly selected = signal<TalentScoreDto | null>(null);

  calculateModel: CalculateTalentScoreRequest = {
    employeeId: '',
    performanceCycleId: '',
    notes: '',
  };

  recalculateModel: RecalculateTalentScoreRequest = {
    employeeId: '',
    performanceCycleId: '',
    notes: '',
  };

  lookupEmployeeId = '';
  lookupCycleId = '';
  readonly lookupResult = signal<TalentScoreDto | null>(null);
  readonly lookupBusy = signal(false);

  ngOnInit(): void {
    this.cyclesApi.getLookup({ take: 200, lang: this.i18n.lang() }).subscribe({
      next: (r) => this.cycles.set(r),
      error: () => this.cycles.set([]),
    });
    this.identityLookups.getEmployees(undefined, 200).subscribe({
      next: (r) => this.employees.set(r),
      error: () => this.employees.set([]),
    });
    this.load();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.ClassificationManage);
  }

  employeeName(id: string): string {
    return this.employees().find((e) => e.id === id)?.name ?? '';
  }

  cycleName(id: string): string {
    return this.cycles().find((c) => c.id === id)?.name ?? id;
  }

  load(): void {
    this.failed.set(false);
    this.busy.set(true);
    const filter: TalentScoreFilterRequest = {
      page: this.page,
      pageSize: this.pageSize,
      employeeId: this.filterEmployeeId.trim() || null,
      performanceCycleId: this.filterPerformanceCycleId || null,
      minFinalScore: this.filterMinFinal,
      maxFinalScore: this.filterMaxFinal,
    };
    this.api.getPaged(filter).subscribe({
      next: (r) => {
        this.result.set(r);
        this.busy.set(false);
      },
      error: () => {
        this.toast.show(this.i18n.t('talentScoresAdmin.toast.loadListFailed'), 'error');
        this.result.set(null);
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
    if (!this.result()?.hasPreviousPage) return;
    this.page--;
    this.load();
  }

  nextPage(): void {
    if (!this.result()?.hasNextPage) return;
    this.page++;
    this.load();
  }

  openDetails(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('talentScoresAdmin.toast.loadDetailsFailed'), 'error'),
    });
  }

  openCalculate(): void {
    this.calculateModel = {
      employeeId: '',
      performanceCycleId: this.filterPerformanceCycleId || '',
      notes: '',
    };
    this.calculateOpen.set(true);
  }

  saveCalculate(): void {
    if (!this.calculateModel.employeeId.trim() || !this.calculateModel.performanceCycleId) {
      this.toast.show(this.i18n.t('talentScoresAdmin.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .calculate({
        employeeId: this.calculateModel.employeeId.trim(),
        performanceCycleId: this.calculateModel.performanceCycleId,
        notes: this.calculateModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('talentScoresAdmin.toast.calculated'), 'success');
          this.calculateOpen.set(false);
          this.page = 1;
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('talentScoresAdmin.toast.calculateFailed'), 'error'),
      });
  }

  openRecalculate(): void {
    this.recalculateModel = {
      employeeId: '',
      performanceCycleId: this.filterPerformanceCycleId || '',
      notes: '',
    };
    this.recalculateOpen.set(true);
  }

  saveRecalculate(): void {
    if (!this.recalculateModel.employeeId.trim() || !this.recalculateModel.performanceCycleId) {
      this.toast.show(this.i18n.t('talentScoresAdmin.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .recalculate({
        employeeId: this.recalculateModel.employeeId.trim(),
        performanceCycleId: this.recalculateModel.performanceCycleId,
        notes: this.recalculateModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('talentScoresAdmin.toast.recalculated'), 'success');
          this.recalculateOpen.set(false);
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('talentScoresAdmin.toast.recalculateFailed'), 'error'),
      });
  }

  runLookup(): void {
    if (!this.lookupEmployeeId.trim() || !this.lookupCycleId) {
      this.toast.show(this.i18n.t('talentScoresAdmin.toast.lookupRequired'), 'error');
      return;
    }
    this.lookupBusy.set(true);
    this.lookupResult.set(null);
    this.api.getByEmployeeCycle(this.lookupEmployeeId.trim(), this.lookupCycleId).subscribe({
      next: (r) => {
        this.lookupResult.set(r);
        this.lookupBusy.set(false);
      },
      error: () => {
        this.lookupResult.set(null);
        this.lookupBusy.set(false);
        this.toast.show(this.i18n.t('talentScoresAdmin.toast.lookupFailed'), 'error');
      },
    });
  }
}
