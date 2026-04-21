import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { TalentClassificationsApiService } from '../../services/talent-classifications-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  ClassifyTalentClassificationRequest,
  ReclassifyTalentClassificationRequest,
  TalentClassificationDto,
  TalentClassificationFilterRequest,
} from '../../shared/models/classification.models';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-talent-classifications-admin-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, DatePipe, LookupSearchComboComponent],
  templateUrl: './talent-classifications-admin-page.component.html',
  styleUrl: './talent-classifications-admin-page.component.scss',
})
export class TalentClassificationsAdminPageComponent implements OnInit {
  private readonly api = inject(TalentClassificationsApiService);
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
  filterNineBoxCode: number | '' = '';

  page = 1;
  readonly pageSize = 20;

  readonly result = signal<PagedResult<TalentClassificationDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly viewMode = signal<ViewMode>('table');

  readonly detailsOpen = signal(false);
  readonly classifyOpen = signal(false);
  readonly reclassifyOpen = signal(false);
  readonly selected = signal<TalentClassificationDto | null>(null);

  classifyModel: ClassifyTalentClassificationRequest = {
    employeeId: '',
    performanceCycleId: '',
    notes: '',
  };

  reclassifyModel: ReclassifyTalentClassificationRequest = {
    employeeId: '',
    performanceCycleId: '',
    notes: '',
  };

  lookupEmployeeId = '';
  lookupCycleId = '';
  readonly lookupResult = signal<TalentClassificationDto | null>(null);
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

  lang(): UiLang {
    return this.i18n.lang();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.ClassificationManage);
  }

  nineBoxLabel(code: number): string {
    return EnumLabels.nineBoxCode(this.lang(), code);
  }

  perfPotLabels(row: TalentClassificationDto): string {
    const p = EnumLabels.criticalityLevel(this.lang(), row.performanceBand);
    const t = EnumLabels.potentialLevel(this.lang(), row.potentialBand);
    return `${p} / ${t}`;
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
    const filter: TalentClassificationFilterRequest = {
      page: this.page,
      pageSize: this.pageSize,
      employeeId: this.filterEmployeeId.trim() || null,
      performanceCycleId: this.filterPerformanceCycleId || null,
      nineBoxCode: this.filterNineBoxCode === '' ? null : Number(this.filterNineBoxCode),
    };
    this.api.getPaged(filter).subscribe({
      next: (r) => {
        this.result.set(r);
        this.busy.set(false);
      },
      error: () => {
        this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.loadListFailed'), 'error');
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
      error: () => this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.loadDetailsFailed'), 'error'),
    });
  }

  openClassify(): void {
    this.classifyModel = { employeeId: '', performanceCycleId: this.filterPerformanceCycleId || '', notes: '' };
    this.classifyOpen.set(true);
  }

  saveClassify(): void {
    if (!this.classifyModel.employeeId.trim() || !this.classifyModel.performanceCycleId) {
      this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .classify({
        employeeId: this.classifyModel.employeeId.trim(),
        performanceCycleId: this.classifyModel.performanceCycleId,
        notes: this.classifyModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.classified'), 'success');
          this.classifyOpen.set(false);
          this.page = 1;
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.classifyFailed'), 'error'),
      });
  }

  openReclassify(): void {
    this.reclassifyModel = { employeeId: '', performanceCycleId: this.filterPerformanceCycleId || '', notes: '' };
    this.reclassifyOpen.set(true);
  }

  saveReclassify(): void {
    if (!this.reclassifyModel.employeeId.trim() || !this.reclassifyModel.performanceCycleId) {
      this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .reclassify({
        employeeId: this.reclassifyModel.employeeId.trim(),
        performanceCycleId: this.reclassifyModel.performanceCycleId,
        notes: this.reclassifyModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.reclassified'), 'success');
          this.reclassifyOpen.set(false);
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.reclassifyFailed'), 'error'),
      });
  }

  runLookup(): void {
    if (!this.lookupEmployeeId.trim() || !this.lookupCycleId) {
      this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.lookupRequired'), 'error');
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
        this.toast.show(this.i18n.t('talentClassificationsAdmin.toast.lookupFailed'), 'error');
      },
    });
  }
}
