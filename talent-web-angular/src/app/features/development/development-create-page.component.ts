import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { CreateDevelopmentPlanRequest } from '../../shared/models/development.models';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-development-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './development-create-page.component.html',
  styleUrl: './development-create-page.component.scss',
})
export class DevelopmentCreatePageComponent implements OnInit {
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  readonly cycles = signal<readonly PerformanceCycleDto[]>([]);

  model: CreateDevelopmentPlanRequest = {
    employeeId: '',
    performanceCycleId: '',
    planTitle: '',
    sourceType: 1,
    targetCompletionDate: null,
    notes: null,
  };

  ngOnInit(): void {
    this.cyclesApi.getPaged({ page: 1, pageSize: 100 }).subscribe({
      next: (p) => this.cycles.set(p.items),
      error: () => this.cycles.set([]),
    });
  }

  cycleLabel(c: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? c.nameAr || c.nameEn : c.nameEn || c.nameAr;
  }

  save(): void {
    this.busy.set(true);
    const body = {
      ...this.model,
      targetCompletionDate: this.model.targetCompletionDate?.toString().trim() || null,
      notes: this.model.notes?.toString().trim() || null,
    };
    this.api.create(body).subscribe({
      next: (plan) => {
        this.busy.set(false);
        this.toast.show('تم إنشاء الخطة', 'success');
        void this.router.navigate(['/development', plan.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإنشاء', 'error');
      },
    });
  }
}
