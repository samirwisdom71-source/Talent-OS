import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { DevelopmentPlanItemPathsApiService } from '../../services/development-plan-item-paths-api.service';
import { DevelopmentPlanItemsApiService } from '../../services/development-plan-items-api.service';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateDevelopmentPlanItemRequest,
  DevelopmentPlanItemDto,
  UpdateDevelopmentPlanItemRequest,
} from '../../shared/models/development-item.models';
import {
  DevelopmentPlanDto,
  DevelopmentPlanItemPathDto,
  DevelopmentPlanLinkDto,
  UpdateDevelopmentPlanRequest,
} from '../../shared/models/development.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { DevelopmentPathImpactChartComponent } from './development-path-impact-chart.component';

type AddPathForm = {
  sortOrder: number;
  title: string;
  description: string | null;
  plannedStartUtc: string | null;
  plannedEndUtc: string | null;
  helperEmployeeId: string;
  helperOrgUnitId: string;
};

@Component({
  selector: 'app-development-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe, DevelopmentPathImpactChartComponent],
  templateUrl: './development-detail-page.component.html',
  styleUrl: './development-detail-page.component.scss',
})
export class DevelopmentDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly itemsApi = inject(DevelopmentPlanItemsApiService);
  private readonly pathsApi = inject(DevelopmentPlanItemPathsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly plan = signal<DevelopmentPlanDto | null>(null);
  readonly items = signal<readonly DevelopmentPlanItemDto[]>([]);
  readonly employees = signal<readonly LookupItemDto[]>([]);
  readonly orgUnits = signal<readonly LookupItemDto[]>([]);
  readonly performanceCycles = signal<readonly LookupItemDto[]>([]);
  readonly failed = signal(false);
  readonly busyRowId = signal<string | null>(null);
  readonly actionBusy = signal(false);
  readonly pathActionBusy = signal(false);
  readonly createItemOpen = signal(false);
  readonly editItemOpen = signal(false);
  readonly editPlanOpen = signal(false);
  readonly addPathOpen = signal(false);
  readonly selectedItem = signal<DevelopmentPlanItemDto | null>(null);
  readonly pathParentItem = signal<DevelopmentPlanItemDto | null>(null);

  /** بنود فُتح فيها مقطع المسارات (الافتراضي: مقفول) */
  readonly pathsSectionOpenByItemId = signal<Record<string, boolean>>({});

  /** local edit buffer per item id */
  progressDraft: Record<string, number> = {};
  createItemModel: CreateDevelopmentPlanItemRequest = this.emptyCreateItemModel();
  editItemModel: UpdateDevelopmentPlanItemRequest = this.emptyUpdateItemModel();
  editPlanModel: UpdateDevelopmentPlanRequest = { planTitle: '', sourceType: 1, targetCompletionDate: null, notes: null };

  addPathForm: AddPathForm = this.emptyAddPathForm();

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.lookups.getEmployees('', 500).subscribe({
      next: (items) => this.employees.set(items),
      error: () => this.employees.set([]),
    });
    this.lookups.getOrganizationUnits('', 400).subscribe({
      next: (items) => this.orgUnits.set(items),
      error: () => this.orgUnits.set([]),
    });
    this.cyclesApi
      .getLookup({
        take: 400,
        lang: this.i18n.lang() === 'ar' ? 'ar' : 'en',
      })
      .subscribe({
        next: (items) => this.performanceCycles.set(items),
        error: () => this.performanceCycles.set([]),
      });
    this.reload(id);
  }

  employeeName(id: string): string {
    return this.employees().find((e) => e.id === id)?.name || id;
  }

  orgUnitName(id: string): string {
    return this.orgUnits().find((u) => u.id === id)?.name || id;
  }

  helperEntityLabel(helperKind: number, entityId: string): string {
    if (helperKind === 1) return this.employeeName(entityId);
    if (helperKind === 2) return this.orgUnitName(entityId);
    return entityId;
  }

  pathHelperKindLabel(k: number): string {
    if (k === 1) return this.i18n.t('development.paths.helperEmployee');
    if (k === 2) return this.i18n.t('development.paths.helperOrgUnit');
    return String(k);
  }

  performanceCycleDisplayName(): string {
    const id = this.plan()?.performanceCycleId;
    if (!id) return '';
    return this.performanceCycles().find((c) => c.id === id)?.name ?? '';
  }

  planLinkTypeLabel(linkType: number): string {
    const key = `development.linkType.${linkType}`;
    const s = this.i18n.t(key);
    return s === key ? this.i18n.t('development.link.target.generic') : s;
  }

  planLinkTargetDescription(link: DevelopmentPlanLinkDto): string {
    const cycle = this.performanceCycleDisplayName();
    let key: string;
    switch (link.linkType) {
      case 1:
        key = 'development.link.target.competencyReq';
        break;
      case 2:
        key = cycle ? 'development.link.target.potentialWithCycle' : 'development.link.target.potential';
        break;
      case 3:
        key = cycle ? 'development.link.target.perfWithCycle' : 'development.link.target.perf';
        break;
      case 4:
        key = 'development.link.target.succession';
        break;
      case 5:
        key = 'development.link.target.successorCand';
        break;
      case 6:
        key = 'development.link.target.classification';
        break;
      default:
        key = 'development.link.target.generic';
    }
    let s = this.i18n.t(key);
    if (cycle && s.includes('{cycle}')) {
      s = s.replace('{cycle}', cycle);
    }
    return s;
  }

  sortedPaths(it: DevelopmentPlanItemDto): DevelopmentPlanItemPathDto[] {
    return [...(it.paths ?? [])].sort((a, b) => a.sortOrder - b.sortOrder);
  }

  togglePathsSection(itemId: string): void {
    this.pathsSectionOpenByItemId.update((m) => ({ ...m, [itemId]: !m[itemId] }));
  }

  pathsSectionOpen(itemId: string): boolean {
    return this.pathsSectionOpenByItemId()[itemId] === true;
  }

  formatPlanDate(iso?: string | null): string {
    if (!iso) return '—';
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return iso;
    return d.toLocaleDateString(this.i18n.lang() === 'ar' ? 'ar' : 'en-GB', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
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

  itemHasIncompletePaths(it: DevelopmentPlanItemDto): boolean {
    const p = it.paths;
    if (!p?.length) return false;
    return !p.every((x) => x.status === 3);
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
          targetCompletionDate: this.toDateInputValue(plan.targetCompletionDate ?? null),
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
    if (this.itemHasIncompletePaths(it)) {
      this.toast.show(this.i18n.t('development.toast.pathsIncomplete'), 'error');
      return;
    }
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

  completePath(it: DevelopmentPlanItemDto, path: DevelopmentPlanItemPathDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.busyRowId.set(path.id);
    this.pathsApi.markCompleted(it.id, path.id).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.paths.pathCompleted'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('development.paths.pathCompleteFailed'), 'error');
      },
    });
  }

  openAddPath(it: DevelopmentPlanItemDto): void {
    this.pathsSectionOpenByItemId.update((m) => ({ ...m, [it.id]: true }));
    this.pathParentItem.set(it);
    const nextOrder = it.paths?.length ? Math.max(...it.paths.map((p) => p.sortOrder)) + 1 : 0;
    this.addPathForm = {
      ...this.emptyAddPathForm(),
      sortOrder: nextOrder,
    };
    this.addPathOpen.set(true);
  }

  saveAddPath(): void {
    const it = this.pathParentItem();
    const pid = this.plan()?.id;
    if (!it || !pid) return;
    const title = this.addPathForm.title.trim();
    if (!title) {
      this.toast.show(this.i18n.t('املأ كل الحقول المطلوبة'), 'error');
      return;
    }
    const helpers: { helperKind: number; helperEntityId: string }[] = [];
    if (this.addPathForm.helperEmployeeId) {
      helpers.push({ helperKind: 1, helperEntityId: this.addPathForm.helperEmployeeId });
    }
    if (this.addPathForm.helperOrgUnitId) {
      helpers.push({ helperKind: 2, helperEntityId: this.addPathForm.helperOrgUnitId });
    }
    this.pathActionBusy.set(true);
    this.pathsApi
      .add(it.id, {
        sortOrder: Number(this.addPathForm.sortOrder) || 0,
        title,
        description: this.addPathForm.description?.trim() || null,
        plannedStartUtc: this.addPathForm.plannedStartUtc?.trim() || null,
        plannedEndUtc: this.addPathForm.plannedEndUtc?.trim() || null,
        helpers: helpers.length ? helpers : undefined,
      })
      .subscribe({
        next: () => {
          this.pathActionBusy.set(false);
          this.addPathOpen.set(false);
          this.toast.show(this.i18n.t('development.paths.pathAdded'), 'success');
          this.reload(pid);
        },
        error: () => {
          this.pathActionBusy.set(false);
          this.toast.show(this.i18n.t('development.paths.pathAddFailed'), 'error');
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
    this.itemsApi
      .create({
        ...this.createItemModel,
        targetDate: this.normalizeOutgoingDate(this.createItemModel.targetDate),
      })
      .subscribe({
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
      targetDate: this.toDateInputValue(it.targetDate),
      status: it.status,
      progressPercentage: it.progressPercentage,
      notes: it.notes ?? null,
    };
    this.editItemOpen.set(true);
  }

  saveEditItem(): void {
    const selected = this.selectedItem();
    const pid = this.plan()?.id;
    if (!selected || !pid) return;
    this.actionBusy.set(true);
    this.itemsApi
      .update(selected.id, {
        ...this.editItemModel,
        targetDate: this.normalizeOutgoingDate(this.editItemModel.targetDate),
      })
      .subscribe({
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
    this.api
      .update(pid, {
        ...this.editPlanModel,
        targetCompletionDate: this.normalizeOutgoingDate(this.editPlanModel.targetCompletionDate ?? null),
      })
      .subscribe({
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

  private emptyAddPathForm(): AddPathForm {
    return {
      sortOrder: 0,
      title: '',
      description: null,
      plannedStartUtc: null,
      plannedEndUtc: null,
      helperEmployeeId: '',
      helperOrgUnitId: '',
    };
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
      status: 1,
      progressPercentage: 0,
      notes: null,
    };
  }

  /** يحوّل تواريخ ISO من الـ API إلى yyyy-MM-DD (مطلوب لحقل type=date وإلا يظهر فارغاً). */
  private toDateInputValue(value: string | null | undefined): string | null {
    if (value == null || value === '') return null;
    const s = String(value).trim();
    if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
    if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10);
    const d = new Date(s);
    if (Number.isNaN(d.getTime())) return null;
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  /** حقل التاريخ الفارغ من الـ input يكون "" — نرسل null للـ API بدل سلسلة فارغة. */
  private normalizeOutgoingDate(value: string | null | undefined): string | null {
    if (value == null || value === '') return null;
    return value;
  }
}
