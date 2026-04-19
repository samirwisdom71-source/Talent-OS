import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { DevelopmentPlanItemsApiService } from '../../services/development-plan-items-api.service';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { DevelopmentPlanItemDto } from '../../shared/models/development-item.models';
import { DevelopmentPlanDto } from '../../shared/models/development.models';
import { I18nService } from '../../shared/services/i18n.service';
import { IdChipComponent } from '../../shared/ui/id-chip.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-development-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, IdChipComponent],
  templateUrl: './development-detail-page.component.html',
  styleUrl: './development-detail-page.component.scss',
})
export class DevelopmentDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly itemsApi = inject(DevelopmentPlanItemsApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly plan = signal<DevelopmentPlanDto | null>(null);
  readonly items = signal<readonly DevelopmentPlanItemDto[]>([]);
  readonly failed = signal(false);
  readonly busyRowId = signal<string | null>(null);

  /** local edit buffer per item id */
  progressDraft: Record<string, number> = {};

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reload(id);
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

  reload(planId: string): void {
    this.failed.set(false);
    forkJoin({
      plan: this.api.getById(planId),
      its: this.itemsApi.getPaged({ page: 1, pageSize: 200, developmentPlanId: planId }),
    }).subscribe({
      next: ({ plan, its }) => {
        this.plan.set(plan);
        this.items.set(its.items);
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
      this.toast.show('أدخل نسبة بين 0 و 100', 'error');
      return;
    }
    this.busyRowId.set(it.id);
    this.itemsApi.updateProgress(it.id, { progressPercentage: pct }).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show('تم تحديث التقدّم', 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show('تعذر التحديث', 'error');
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
        this.toast.show('تم الإكمال', 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show('تعذر الإكمال', 'error');
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
        this.toast.show('تم الحذف', 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show('تعذر الحذف', 'error');
      },
    });
  }
}
