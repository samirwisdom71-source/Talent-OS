import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { SuccessorCandidatesApiService } from '../../services/successor-candidates-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateSuccessorCandidateRequest,
  SuccessorCandidateDto,
  UpdateSuccessorCandidateRequest,
} from '../../shared/models/successor-candidate.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-successor-candidates-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, RouterLink, LookupSearchComboComponent],
  templateUrl: './successor-candidates-page.component.html',
  styleUrl: './successor-candidates-page.component.scss',
})
export class SuccessorCandidatesPageComponent implements OnInit {
  private readonly api = inject(SuccessorCandidatesApiService);
  private readonly successionApi = inject(SuccessionApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;

  readonly plans = signal<LookupItemDto[]>([]);
  readonly employees = signal<LookupItemDto[]>([]);

  filterSuccessionPlanId = '';
  page = 1;
  readonly pageSize = 20;

  readonly result = signal<PagedResult<SuccessorCandidateDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly viewMode = signal<ViewMode>('table');

  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly selected = signal<SuccessorCandidateDto | null>(null);

  createModel: CreateSuccessorCandidateRequest = {
    successionPlanId: '',
    employeeId: '',
    readinessLevel: 2,
    rankOrder: 1,
    isPrimarySuccessor: false,
    notes: '',
  };

  editId: string | null = null;
  editModel: UpdateSuccessorCandidateRequest = {
    readinessLevel: 2,
    rankOrder: 1,
    isPrimarySuccessor: false,
    notes: '',
  };

  ngOnInit(): void {
    this.loadPlans();
    this.loadEmployees();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.SuccessionManage);
  }

  readiness(v: number): string {
    return EnumLabels.readinessLevel(this.lang(), v);
  }

  private loadPlans(): void {
    this.successionApi.getSuccessionPlansLookup({ take: 300 }).subscribe({
      next: (r) => this.plans.set(r),
      error: () => this.plans.set([]),
    });
  }

  private loadEmployees(): void {
    this.identityLookups.getEmployees(undefined, 200).subscribe({
      next: (r) => this.employees.set(r),
      error: () => this.employees.set([]),
    });
  }

  employeeName(id: string): string {
    return this.employees().find((e) => e.id === id)?.name ?? '';
  }

  planName(id: string): string {
    return this.plans().find((p) => p.id === id)?.name ?? '';
  }

  applyPlanFilter(): void {
    this.page = 1;
    this.load();
  }

  onPlanFilterChange(id: string): void {
    this.filterSuccessionPlanId = id;
    this.applyPlanFilter();
  }

  load(): void {
    if (!this.filterSuccessionPlanId) {
      this.result.set(null);
      this.failed.set(false);
      return;
    }
    this.failed.set(false);
    this.busy.set(true);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        successionPlanId: this.filterSuccessionPlanId,
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.busy.set(false);
        },
        error: () => {
          this.toast.show(this.i18n.t('successorCandidates.toast.loadListFailed'), 'error');
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
    if (!this.filterSuccessionPlanId) {
      this.toast.show(this.i18n.t('successorCandidates.toast.pickPlanFirst'), 'error');
      return;
    }
    const nextRank = (this.result()?.items.length ?? 0) + 1;
    this.createModel = {
      successionPlanId: this.filterSuccessionPlanId,
      employeeId: '',
      readinessLevel: 2,
      rankOrder: nextRank,
      isPrimarySuccessor: false,
      notes: '',
    };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    if (!this.createModel.employeeId.trim()) {
      this.toast.show(this.i18n.t('successorCandidates.toast.requiredEmployee'), 'error');
      return;
    }
    this.api
      .create({
        ...this.createModel,
        employeeId: this.createModel.employeeId.trim(),
        notes: this.createModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('successorCandidates.toast.created'), 'success');
          this.createOpen.set(false);
          this.page = 1;
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('successorCandidates.toast.createFailed'), 'error'),
      });
  }

  openDetails(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('successorCandidates.toast.loadDetailsFailed'), 'error'),
    });
  }

  openEdit(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.editId = r.id;
        this.selected.set(r);
        this.editModel = {
          readinessLevel: r.readinessLevel,
          rankOrder: r.rankOrder,
          isPrimarySuccessor: r.isPrimarySuccessor,
          notes: r.notes ?? '',
        };
        this.editOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('successorCandidates.toast.loadEditFailed'), 'error'),
    });
  }

  saveEdit(): void {
    if (!this.editId) return;
    this.api
      .update(this.editId, {
        ...this.editModel,
        readinessLevel: Number(this.editModel.readinessLevel),
        rankOrder: Number(this.editModel.rankOrder),
        notes: this.editModel.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show(this.i18n.t('successorCandidates.toast.updated'), 'success');
          this.editOpen.set(false);
          this.load();
        },
        error: () => this.toast.show(this.i18n.t('successorCandidates.toast.updateFailed'), 'error'),
      });
  }

  markPrimary(c: SuccessorCandidateDto): void {
    if (!this.canManage()) return;
    this.api.markPrimary(c.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('successorCandidates.toast.primarySet'), 'success');
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('successorCandidates.toast.primaryFailed'), 'error'),
    });
  }

  remove(c: SuccessorCandidateDto): void {
    if (!this.canManage()) return;
    this.api.remove(c.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('successorCandidates.toast.deleted'), 'success');
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('successorCandidates.toast.deleteFailed'), 'error'),
    });
  }
}
