import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateSuccessionPlanRequest,
  SuccessionPlanDto,
  SuccessionPlanFilterRequest,
  UpdateSuccessionPlanRequest,
} from '../../shared/models/succession.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-succession-plans-list-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, RouterLink, DecimalPipe],
  templateUrl: './succession-plans-list-page.component.html',
  styleUrl: './succession-plans-list-page.component.scss',
})
export class SuccessionPlansListPageComponent implements OnInit {
  private readonly api = inject(SuccessionApiService);
  private readonly cyclesLookup = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;

  readonly result = signal<PagedResult<SuccessionPlanDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly viewMode = signal<ViewMode>('table');

  readonly criticalPositions = signal<LookupItemDto[]>([]);
  readonly cycles = signal<LookupItemDto[]>([]);

  page = 1;
  readonly pageSize = 20;
  filterCriticalPositionId = '';
  filterPerformanceCycleId = '';

  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly selected = signal<SuccessionPlanDto | null>(null);

  createModel: CreateSuccessionPlanRequest = {
    criticalPositionId: '',
    performanceCycleId: '',
    planName: '',
    notes: '',
  };

  editId: string | null = null;
  editModel: UpdateSuccessionPlanRequest = { planName: '', notes: '' };

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.SuccessionManage);
  }

  planStatusLabel(s: number): string {
    return EnumLabels.successionPlanStatus(this.lang(), s);
  }

  labelFrom(list: LookupItemDto[], id: string): string {
    return list.find((x) => x.id === id)?.name ?? id;
  }

  private loadLookups(): void {
    this.api.getCriticalPositionsLookup({ take: 300, lang: this.lang(), activeOnly: true }).subscribe({
      next: (r) => this.criticalPositions.set(r),
      error: () => this.criticalPositions.set([]),
    });
    this.cyclesLookup.getLookup({ take: 200, lang: this.lang() }).subscribe({
      next: (r) => this.cycles.set(r),
      error: () => this.cycles.set([]),
    });
  }

  load(): void {
    this.failed.set(false);
    this.busy.set(true);
    const filter: SuccessionPlanFilterRequest = {
      page: this.page,
      pageSize: this.pageSize,
      criticalPositionId: this.filterCriticalPositionId || null,
      performanceCycleId: this.filterPerformanceCycleId || null,
    };
    this.api.getPlansPaged(filter).subscribe({
      next: (r) => {
        this.result.set(r);
        this.busy.set(false);
      },
      error: () => {
        this.toast.show(this.i18n.t('successionPlans.toast.loadListFailed'), 'error');
        this.result.set(null);
        this.busy.set(false);
        this.failed.set(true);
      },
    });
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

  openCreate(): void {
    this.createModel = { criticalPositionId: '', performanceCycleId: '', planName: '', notes: '' };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    if (!this.createModel.criticalPositionId || !this.createModel.performanceCycleId || !this.createModel.planName.trim()) {
      this.toast.show(this.i18n.t('successionPlans.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .createPlan({
        ...this.createModel,
        planName: this.createModel.planName.trim(),
        notes: this.createModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('successionPlans.toast.created'), 'success');
          this.createOpen.set(false);
          this.page = 1;
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('successionPlans.toast.createFailed'), 'error'),
      });
  }

  openDetails(id: string): void {
    this.api.getPlanById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('successionPlans.toast.loadDetailsFailed'), 'error'),
    });
  }

  openEdit(id: string): void {
    this.api.getPlanById(id).subscribe({
      next: (r) => {
        this.editId = r.id;
        this.selected.set(r);
        this.editModel = { planName: r.planName, notes: r.notes ?? '' };
        this.editOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('successionPlans.toast.loadEditFailed'), 'error'),
    });
  }

  saveEdit(): void {
    if (!this.editId) return;
    if (!this.editModel.planName.trim()) {
      this.toast.show(this.i18n.t('successionPlans.toast.requiredFields'), 'error');
      return;
    }
    this.api
      .updatePlan(this.editId, {
        planName: this.editModel.planName.trim(),
        notes: this.editModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('successionPlans.toast.updated'), 'success');
          this.editOpen.set(false);
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('successionPlans.toast.updateFailed'), 'error'),
      });
  }

  canActivate(row: SuccessionPlanDto): boolean {
    return row.status === 1;
  }

  canClose(row: SuccessionPlanDto): boolean {
    return row.status === 2;
  }

  activate(row: SuccessionPlanDto): void {
    if (!this.canManage() || !this.canActivate(row)) return;
    this.api.activatePlan(row.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('successionPlans.toast.activated'), 'success');
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('successionPlans.toast.activateFailed'), 'error'),
    });
  }

  close(row: SuccessionPlanDto): void {
    if (!this.canManage() || !this.canClose(row)) return;
    this.api.closePlan(row.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('successionPlans.toast.closed'), 'success');
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('successionPlans.toast.closeFailed'), 'error'),
    });
  }
}
