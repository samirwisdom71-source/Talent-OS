import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateCriticalPositionRequest,
  CriticalPositionDto,
  UpdateCriticalPositionRequest,
} from '../../shared/models/succession.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type CpViewMode = 'table' | 'cards';
type ActiveScopeFilter = 'active' | 'all';

@Component({
  selector: 'app-critical-positions-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './critical-positions-page.component.html',
  styleUrl: './critical-positions-page.component.scss',
})
export class CriticalPositionsPageComponent implements OnInit {
  private readonly api = inject(SuccessionApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);

  readonly PermissionCodes = PermissionCodes;

  readonly result = signal<PagedResult<CriticalPositionDto> | null>(null);
  readonly positions = signal<LookupItemDto[]>([]);
  readonly busy = signal(false);
  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly archiveConfirmOpen = signal(false);
  readonly pendingArchiveRow = signal<CriticalPositionDto | null>(null);
  readonly selected = signal<CriticalPositionDto | null>(null);

  page = 1;
  readonly pageSize = 20;
  filterPositionId = '';
  filterActiveScope: ActiveScopeFilter = 'active';
  viewMode: CpViewMode = 'table';

  createModel: CreateCriticalPositionRequest = {
    positionId: '',
    criticalityLevel: 2,
    riskLevel: 2,
    notes: '',
  };

  editModel: UpdateCriticalPositionRequest = {
    criticalityLevel: 2,
    riskLevel: 2,
    notes: '',
  };

  ngOnInit(): void {
    this.loadPositions();
    this.load();
  }

  private uiLang(): UiLang {
    return this.i18n.lang();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.SuccessionManage);
  }

  private loadPositions(): void {
    this.lookups.getPositions('', 300).subscribe({
      next: (r) => this.positions.set(r),
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.loadPositionsFailed'), 'error'),
    });
  }

  load(): void {
    this.busy.set(true);
    const activeOnly = this.filterActiveScope === 'active' ? true : undefined;
    this.api
      .getCriticalPositionsPaged({
        page: this.page,
        pageSize: this.pageSize,
        positionId: this.filterPositionId || null,
        activeOnly,
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.busy.set(false);
        },
        error: () => {
          this.toast.show(this.i18n.t('criticalPositions.toast.loadListFailed'), 'error');
          this.busy.set(false);
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

  labelFrom(list: LookupItemDto[], id: string): string {
    return list.find((x) => x.id === id)?.name ?? id;
  }

  criticalityLabel(v: number): string {
    return EnumLabels.criticalityLevel(this.uiLang(), v);
  }

  riskLabel(v: number): string {
    return EnumLabels.successionRiskLevel(this.uiLang(), v);
  }

  recordStatusLabel(v: number): string {
    return EnumLabels.recordStatus(this.uiLang(), v);
  }

  isRowActive(row: CriticalPositionDto): boolean {
    return row.recordStatus === 1;
  }

  openCreate(): void {
    this.createModel = {
      positionId: '',
      criticalityLevel: 2,
      riskLevel: 2,
      notes: '',
    };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    if (!this.createModel.positionId) {
      this.toast.show(this.i18n.t('criticalPositions.toast.requiredFields'), 'error');
      return;
    }
    const body: CreateCriticalPositionRequest = {
      positionId: this.createModel.positionId,
      criticalityLevel: Number(this.createModel.criticalityLevel),
      riskLevel: Number(this.createModel.riskLevel),
      notes: this.createModel.notes?.trim() || null,
    };
    this.api.createCriticalPosition(body).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('criticalPositions.toast.created'), 'success');
        this.createOpen.set(false);
        this.page = 1;
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.createFailed'), 'error'),
    });
  }

  openDetails(id: string): void {
    this.api.getCriticalPositionById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.loadDetailsFailed'), 'error'),
    });
  }

  openEdit(id: string): void {
    this.api.getCriticalPositionById(id).subscribe({
      next: (r) => {
        if (r.recordStatus !== 1) {
          this.toast.show(this.i18n.t('criticalPositions.toast.readOnly'), 'error');
          return;
        }
        this.selected.set(r);
        this.editModel = {
          criticalityLevel: r.criticalityLevel,
          riskLevel: r.riskLevel,
          notes: r.notes ?? '',
        };
        this.editOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.loadEditFailed'), 'error'),
    });
  }

  saveEdit(): void {
    const row = this.selected();
    if (!row) return;
    const body: UpdateCriticalPositionRequest = {
      criticalityLevel: Number(this.editModel.criticalityLevel),
      riskLevel: Number(this.editModel.riskLevel),
      notes: this.editModel.notes?.trim() || null,
    };
    this.api.updateCriticalPosition(row.id, body).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('criticalPositions.toast.updated'), 'success');
        this.editOpen.set(false);
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.updateFailed'), 'error'),
    });
  }

  openArchiveConfirm(row: CriticalPositionDto): void {
    if (!this.canManage() || !this.isRowActive(row)) return;
    this.pendingArchiveRow.set(row);
    this.archiveConfirmOpen.set(true);
  }

  closeArchiveConfirm(): void {
    this.archiveConfirmOpen.set(false);
    this.pendingArchiveRow.set(null);
  }

  executeArchive(): void {
    const row = this.pendingArchiveRow();
    if (!row) return;
    this.api.deactivateCriticalPosition(row.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('criticalPositions.toast.deactivated'), 'success');
        this.closeArchiveConfirm();
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('criticalPositions.toast.deactivateFailed'), 'error'),
    });
  }
}
