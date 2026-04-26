import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { CreateDevelopmentPlanRequest } from '../../shared/models/development.models';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-development-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe],
  templateUrl: './development-create-page.component.html',
  styleUrl: './development-create-page.component.scss',
})
export class DevelopmentCreatePageComponent implements OnInit {
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  readonly suggestBusy = signal(false);
  readonly cycles = signal<readonly LookupItemDto[]>([]);
  readonly employees = signal<readonly LookupItemDto[]>([]);

  readonly sourceOptions: readonly number[] = [1, 2, 3, 4, 5];

  model: CreateDevelopmentPlanRequest = {
    employeeId: '',
    performanceCycleId: '',
    planTitle: '',
    sourceType: 1,
    isSystemSuggested: false,
    targetCompletionDate: null,
    notes: null,
    links: undefined,
    structuredItems: undefined,
  };

  ngOnInit(): void {
    this.lookups.getEmployees('', 200).subscribe({
      next: (items) => this.employees.set(items),
      error: () => this.employees.set([]),
    });
    this.cyclesApi.getLookup({ lang: this.i18n.lang(), take: 200 }).subscribe({
      next: (items) => this.cycles.set(items),
      error: () => this.cycles.set([]),
    });
  }

  sourceType(v: number): string {
    return EnumLabels.developmentSourceType(this.i18n.lang(), v);
  }

  structuredPreview(): { items: number; paths: number } {
    const items = this.model.structuredItems?.length ?? 0;
    let paths = 0;
    for (const it of this.model.structuredItems ?? []) {
      paths += it.paths?.length ?? 0;
    }
    return { items, paths };
  }

  suggestFromSystem(): void {
    if (!this.model.employeeId || !this.model.performanceCycleId) {
      this.toast.show(this.i18n.t('development.create.suggestNeedSubject'), 'error');
      return;
    }
    this.suggestBusy.set(true);
    this.api
      .suggest({
        employeeId: this.model.employeeId,
        performanceCycleId: this.model.performanceCycleId,
        sourceType: this.model.sourceType,
      })
      .subscribe({
        next: (s) => {
          this.suggestBusy.set(false);
          this.model.planTitle = s.planTitle;
          this.model.notes = s.notes ?? null;
          this.model.structuredItems = s.items.length ? [...s.items] : undefined;
          this.model.links = s.links.length ? [...s.links] : undefined;
          this.model.isSystemSuggested = true;
          this.toast.show(this.i18n.t('development.create.suggestOk'), 'success');
        },
        error: () => {
          this.suggestBusy.set(false);
          this.toast.show(this.i18n.t('development.create.suggestFail'), 'error');
        },
      });
  }

  clearStructuredDraft(): void {
    this.model.structuredItems = undefined;
    this.model.links = undefined;
    this.model.isSystemSuggested = false;
  }

  save(): void {
    if (!this.model.employeeId || !this.model.performanceCycleId || !this.model.planTitle.trim()) {
      this.toast.show(this.i18n.t('املأ كل الحقول المطلوبة'), 'error');
      return;
    }
    this.busy.set(true);
    const body: CreateDevelopmentPlanRequest = {
      ...this.model,
      targetCompletionDate: this.model.targetCompletionDate?.toString().trim() || null,
      notes: this.model.notes?.toString().trim() || null,
      isSystemSuggested: this.model.isSystemSuggested ?? false,
      structuredItems: this.model.structuredItems?.length ? this.model.structuredItems : undefined,
      links: this.model.links?.length ? this.model.links : undefined,
    };
    this.api.create(body).subscribe({
      next: (plan) => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('تم إنشاء الخطة'), 'success');
        void this.router.navigate(['/development', plan.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('تعذر الإنشاء'), 'error');
      },
    });
  }
}
