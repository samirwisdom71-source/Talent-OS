import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { DevelopmentPlanItemsApiService } from '../../services/development-plan-items-api.service';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateDevelopmentPlanItemRequest,
  DevelopmentPlanItemDto,
  UpdateDevelopmentPlanItemRequest,
} from '../../shared/models/development-item.models';
import { DevelopmentPlanDto, UpdateDevelopmentPlanRequest } from '../../shared/models/development.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-development-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe],
  templateUrl: './development-detail-page.component.html',
  styleUrl: './development-detail-page.component.scss',
})
export class DevelopmentDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly itemsApi = inject(DevelopmentPlanItemsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly plan = signal<DevelopmentPlanDto | null>(null);
  readonly items = signal<readonly DevelopmentPlanItemDto[]>([]);
  readonly employees = signal<readonly LookupItemDto[]>([]);
  readonly failed = signal(false);
  readonly busyRowId = signal<string | null>(null);
  readonly actionBusy = signal(false);
  readonly createItemOpen = signal(false);
  readonly editItemOpen = signal(false);
  readonly editPlanOpen = signal(false);
  readonly selectedItem = signal<DevelopmentPlanItemDto | null>(null);

  viewMode: ViewMode = 'table';

  /** local edit buffer per item id */
  progressDraft: Record<string, number> = {};
  createItemModel: CreateDevelopmentPlanItemRequest = this.emptyCreateItemModel();
  editItemModel: UpdateDevelopmentPlanItemRequest = this.emptyUpdateItemModel();
  editPlanModel: UpdateDevelopmentPlanRequest = { planTitle: '', sourceType: 1, targetCompletionDate: null, notes: null };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.lookups.getEmployees('', 500).subscribe({
      next: (items) => this.employees.set(items),
      error: () => this.employees.set([]),
    });
    this.reload(id);
  }

  employeeName(id: string): string {
    return this.employees().find((e) => e.id === id)?.name || id;
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  planStatus(s: number): string {
    return EnumLabels.developmentPlanStatus(this.lang(), s);
  }

  itemStatus(s: number): string {
    return EnumLabels.developmentItemStatus(this.lang(), s);
  }

  itemType(t: number): string {
    return EnumLabels.developmentItemType(this.lang(), t);
  }

  sourceType(t: number): string {
    return EnumLabels.developmentSourceType(this.lang(), t);
  }

  reload(planId: string): void {
    this.failed.set(false);
    forkJoin({
      plan: this.api.getById(planId),
      its: this.itemsApi.getPaged({ page: 1, pageSize: 200, developmentPlanId: planId }),
    }).subscribe({
      next: ({ plan, its }) => {
        this.plan.set(plan);
        this.items.set(its.items);
        this.editPlanModel = {
          planTitle: plan.planTitle,
          sourceType: plan.sourceType,
          targetCompletionDate: plan.targetCompletionDate ?? null,
          notes: plan.notes ?? null,
        };
        const draft: Record<string, number> = {};
        for (const it of its.items) {
          draft[it.id] = it.progressPercentage;
        }
        this.progressDraft = draft;
        this.failed.set(false);
      },
      error: () => {
        this.plan.set(null);
        this.items.set([]);
        this.failed.set(true);
      },
    });
  }

  progressFor(it: DevelopmentPlanItemDto): number {
    return this.progressDraft[it.id] ?? it.progressPercentage;
  }

  setProgress(it: DevelopmentPlanItemDto, v: number): void {
    this.progressDraft = { ...this.progressDraft, [it.id]: v };
  }

  saveProgress(it: DevelopmentPlanItemDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    const pct = Number(this.progressDraft[it.id] ?? it.progressPercentage);
    if (Number.isNaN(pct) || pct < 0 || pct > 100) {
      this.toast.show(this.i18n.t('development.toast.progressRange'), 'error');
      return;
    }
    this.busyRowId.set(it.id);
    this.itemsApi.updateProgress(it.id, { progressPercentage: pct }).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.toast.progressUpdated'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.toast.updateFailed'), 'error');
      },
    });
  }

  markDone(it: DevelopmentPlanItemDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.busyRowId.set(it.id);
    this.itemsApi.markCompleted(it.id).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.toast.itemCompleted'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.toast.completeFailed'), 'error');
      },
    });
  }

  remove(it: DevelopmentPlanItemDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.busyRowId.set(it.id);
    this.itemsApi.remove(it.id).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تم الحذف'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تعذر الحذف'), 'error');
      },
    });
  }

  openCreateItem(): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.createItemModel = {
      ...this.emptyCreateItemModel(),
      developmentPlanId: pid,
    };
    this.createItemOpen.set(true);
  }

  saveCreateItem(): void {
    if (!this.createItemModel.title.trim()) {
      this.toast.show(this.i18n.t('املأ كل الحقول المطلوبة'), 'error');
      return;
    }
    const pid = this.plan()?.id;
    if (!pid) return;
    this.actionBusy.set(true);
    this.itemsApi.create(this.createItemModel).subscribe({
      next: () => {
        this.actionBusy.set(false);
        this.createItemOpen.set(false);
        this.toast.show(this.i18n.t('development.toast.itemCreated'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.actionBusy.set(false);
        this.toast.show(this.i18n.t('development.toast.createFailed'), 'error');
      },
    });
  }

  openEditItem(it: DevelopmentPlanItemDto): void {
    this.selectedItem.set(it);
    this.editItemModel = {
      title: it.title,
      description: it.description ?? null,
      itemType: it.itemType,
      relatedCompetencyId: it.relatedCompetencyId ?? null,
      targetDate: it.targetDate ?? null,
      notes: it.notes ?? null,
    };
    this.editItemOpen.set(true);
  }

  saveEditItem(): void {
    const selected = this.selectedItem();
    const pid = this.plan()?.id;
    if (!selected || !pid) return;
    this.actionBusy.set(true);
    this.itemsApi.update(selected.id, this.editItemModel).subscribe({
      next: () => {
        this.actionBusy.set(false);
        this.editItemOpen.set(false);
        this.toast.show(this.i18n.t('تم التحديث'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.actionBusy.set(false);
        this.toast.show(this.i18n.t('تعذر التحديث'), 'error');
      },
    });
  }

  savePlanEdit(): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.actionBusy.set(true);
    this.api.update(pid, this.editPlanModel).subscribe({
      next: () => {
        this.actionBusy.set(false);
        this.editPlanOpen.set(false);
        this.toast.show(this.i18n.t('تم التحديث'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.actionBusy.set(false);
        this.toast.show(this.i18n.t('تعذر التحديث'), 'error');
      },
    });
  }

  activatePlan(): void {
    this.runPlanAction('activate');
  }

  completePlan(): void {
    this.runPlanAction('complete');
  }

  cancelPlan(): void {
    this.runPlanAction('cancel');
  }

  private runPlanAction(action: 'activate' | 'complete' | 'cancel'): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.actionBusy.set(true);
    const req =
      action === 'activate' ? this.api.activate(pid) : action === 'complete' ? this.api.complete(pid) : this.api.cancel(pid);
    req.subscribe({
      next: () => {
        this.actionBusy.set(false);
        this.toast.show(this.i18n.t('development.toast.planActionSuccess'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.actionBusy.set(false);
        this.toast.show(this.i18n.t('development.toast.planActionFailed'), 'error');
      },
    });
  }

  private emptyCreateItemModel(): CreateDevelopmentPlanItemRequest {
    return {
      developmentPlanId: '',
      title: '',
      description: null,
      itemType: 1,
      relatedCompetencyId: null,
      targetDate: null,
      notes: null,
    };
  }

  private emptyUpdateItemModel(): UpdateDevelopmentPlanItemRequest {
    return {
      title: '',
      description: null,
      itemType: 1,
      relatedCompetencyId: null,
      targetDate: null,
      notes: null,
    };
  }
}
