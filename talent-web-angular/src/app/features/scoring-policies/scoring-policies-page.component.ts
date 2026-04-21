import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { ScoringPoliciesApiService } from '../../services/scoring-policies-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateScoringPolicyRequest,
  ScoringPolicyDto,
  UpdateScoringPolicyRequest,
} from '../../shared/models/scoring-policy.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-scoring-policies-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, DatePipe, DecimalPipe],
  templateUrl: './scoring-policies-page.component.html',
  styleUrl: './scoring-policies-page.component.scss',
})
export class ScoringPoliciesPageComponent implements OnInit {
  private readonly api = inject(ScoringPoliciesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;

  readonly result = signal<PagedResult<ScoringPolicyDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly viewMode = signal<ViewMode>('table');

  search = '';
  page = 1;
  readonly pageSize = 20;

  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly selected = signal<ScoringPolicyDto | null>(null);

  createModel: CreateScoringPolicyRequest = this.emptyModel();
  editId: string | null = null;
  editModel: UpdateScoringPolicyRequest = this.emptyModel();

  ngOnInit(): void {
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  canManage(): boolean {
    return this.auth.hasPermission(PermissionCodes.ScoringManage);
  }

  recordStatusLabel(v: number): string {
    return EnumLabels.recordStatus(this.lang(), v);
  }

  load(): void {
    this.failed.set(false);
    this.busy.set(true);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search || null,
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.busy.set(false);
        },
        error: () => {
          this.toast.show(this.i18n.t('scoringPolicies.toast.loadListFailed'), 'error');
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
    this.createModel = this.emptyModel();
    this.createOpen.set(true);
  }

  saveCreate(): void {
    if (!this.valid(this.createModel)) {
      this.toast.show(this.i18n.t('scoringPolicies.toast.requiredFields'), 'error');
      return;
    }
    this.api.create(this.normalize(this.createModel)).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('scoringPolicies.toast.created'), 'success');
        this.createOpen.set(false);
        this.page = 1;
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('scoringPolicies.toast.createFailed'), 'error'),
    });
  }

  openDetails(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('scoringPolicies.toast.loadDetailsFailed'), 'error'),
    });
  }

  openEdit(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.editId = r.id;
        this.selected.set(r);
        this.editModel = {
          name: r.name,
          version: r.version,
          performanceWeight: r.performanceWeight,
          potentialWeight: r.potentialWeight,
          effectiveFromUtc: r.effectiveFromUtc,
          notes: r.notes ?? '',
        };
        this.editOpen.set(true);
      },
      error: () => this.toast.show(this.i18n.t('scoringPolicies.toast.loadEditFailed'), 'error'),
    });
  }

  saveEdit(): void {
    if (!this.editId) return;
    if (!this.valid(this.editModel)) {
      this.toast.show(this.i18n.t('scoringPolicies.toast.requiredFields'), 'error');
      return;
    }
    this.api.update(this.editId, this.normalize(this.editModel)).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('scoringPolicies.toast.updated'), 'success');
        this.editOpen.set(false);
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('scoringPolicies.toast.updateFailed'), 'error'),
    });
  }

  canActivate(row: ScoringPolicyDto): boolean {
    // RecordStatus.Active = 1
    return row.recordStatus !== 1;
  }

  activate(row: ScoringPolicyDto): void {
    if (!this.canManage() || !this.canActivate(row)) return;
    this.api.activate(row.id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('scoringPolicies.toast.activated'), 'success');
        this.load();
      },
      error: () => this.toast.show(this.i18n.t('scoringPolicies.toast.activateFailed'), 'error'),
    });
  }

  effectiveDateInput(model: { effectiveFromUtc: string }): string {
    return (model.effectiveFromUtc || '').slice(0, 10);
  }

  setEffectiveDateFromInput(model: { effectiveFromUtc: string }, value: string): void {
    model.effectiveFromUtc = `${value}T00:00:00.000Z`;
  }

  private emptyModel(): CreateScoringPolicyRequest {
    const today = new Date().toISOString().slice(0, 10);
    return {
      name: '',
      version: '',
      performanceWeight: 0.6,
      potentialWeight: 0.4,
      effectiveFromUtc: `${today}T00:00:00.000Z`,
      notes: '',
    };
  }

  private valid(m: CreateScoringPolicyRequest): boolean {
    return !!m.name?.trim() && !!m.version?.trim();
  }

  private normalize<T extends CreateScoringPolicyRequest>(m: T): T {
    return {
      ...m,
      name: m.name.trim(),
      version: m.version.trim(),
      performanceWeight: Number(m.performanceWeight),
      potentialWeight: Number(m.potentialWeight),
      notes: m.notes?.trim() || null,
    };
  }
}

