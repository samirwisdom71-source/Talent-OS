import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { CreateSuccessionPlanRequest, CriticalPositionDto } from '../../shared/models/succession.models';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-succession-plan-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './succession-plan-create-page.component.html',
  styleUrl: '../development/development-create-page.component.scss',
})
export class SuccessionPlanCreatePageComponent implements OnInit {
  private readonly api = inject(SuccessionApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  readonly cycles = signal<readonly PerformanceCycleDto[]>([]);
  readonly positions = signal<readonly CriticalPositionDto[]>([]);

  model: CreateSuccessionPlanRequest = {
    criticalPositionId: '',
    performanceCycleId: '',
    planName: '',
    notes: null,
  };

  ngOnInit(): void {
    forkJoin({
      cy: this.cyclesApi.getPaged({ page: 1, pageSize: 100 }),
      cp: this.api.getCriticalPositionsPaged({ page: 1, pageSize: 100, activeOnly: true }),
    }).subscribe({
      next: ({ cy, cp }) => {
        this.cycles.set(cy.items);
        this.positions.set(cp.items);
      },
      error: () => {
        this.cycles.set([]);
        this.positions.set([]);
      },
    });
  }

  cycleLabel(c: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? c.nameAr || c.nameEn : c.nameEn || c.nameAr;
  }

  save(): void {
    this.busy.set(true);
    this.api.createPlan(this.model).subscribe({
      next: (p) => {
        this.busy.set(false);
        this.toast.show('تم إنشاء الخطة', 'success');
        void this.router.navigate(['/succession/plans', p.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإنشاء', 'error');
      },
    });
  }
}
