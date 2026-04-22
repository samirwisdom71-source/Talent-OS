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
  readonly cycles = signal<readonly LookupItemDto[]>([]);
  readonly employees = signal<readonly LookupItemDto[]>([]);

  readonly sourceOptions: readonly number[] = [1, 2, 3, 4, 5];

  model: CreateDevelopmentPlanRequest = {
    employeeId: '',
    performanceCycleId: '',
    planTitle: '',
    sourceType: 1,
    targetCompletionDate: null,
    notes: null,
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

  save(): void {
    if (!this.model.employeeId || !this.model.performanceCycleId || !this.model.planTitle.trim()) {
      this.toast.show(this.i18n.t('املأ كل الحقول المطلوبة'), 'error');
      return;
    }
    this.busy.set(true);
    const body = {
      ...this.model,
      targetCompletionDate: this.model.targetCompletionDate?.toString().trim() || null,
      notes: this.model.notes?.toString().trim() || null,
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
