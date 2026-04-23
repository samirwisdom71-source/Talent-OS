import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/auth/auth.service';
import { IntelligenceApiService } from '../../services/intelligence-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PermissionCodes as PermissionCodesConst } from '../../shared/models/permission-codes';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';

@Component({
  selector: 'app-intelligence-overview-page',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, TranslatePipe, LookupSearchComboComponent],
  templateUrl: './intelligence-overview-page.component.html',
  styleUrl: './intelligence-overview-page.component.scss',
})
export class IntelligenceOverviewPageComponent implements OnInit {
  private readonly api = inject(IntelligenceApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly fb = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);
  readonly auth = inject(AuthService);

  readonly PermissionCodes = PermissionCodesConst;

  readonly insightTotal = signal<number | null>(null);
  readonly recommendationTotal = signal<number | null>(null);
  readonly failed = signal(false);
  readonly cycles = signal<readonly PerformanceCycleDto[]>([]);

  readonly genEmployeeBusy = signal(false);
  readonly genCycleBusy = signal(false);

  readonly employeeForm = this.fb.nonNullable.group({
    employeeId: ['', [Validators.required]],
    performanceCycleId: ['', [Validators.required]],
    target: [3 as 1 | 2 | 3, [Validators.required]],
  });

  readonly cycleForm = this.fb.nonNullable.group({
    performanceCycleId: ['', [Validators.required]],
  });

  ngOnInit(): void {
    forkJoin({
      i: this.api.getInsightsPaged({ page: 1, pageSize: 1 }),
      r: this.api.getRecommendationsPaged({ page: 1, pageSize: 1 }),
    }).subscribe({
      next: ({ i, r }) => {
        this.insightTotal.set(i.totalCount);
        this.recommendationTotal.set(r.totalCount);
        this.failed.set(false);
      },
      error: () => {
        this.insightTotal.set(null);
        this.recommendationTotal.set(null);
        this.failed.set(true);
      },
    });

    this.cyclesApi.getPaged({ page: 1, pageSize: 100 }).subscribe({
      next: (p) => this.cycles.set(p.items),
      error: () => this.cycles.set([]),
    });
  }

  runEmployeeGeneration(): void {
    if (this.employeeForm.invalid || !this.auth.hasPermission(PermissionCodesConst.IntelligenceGenerate)) return;
    this.genEmployeeBusy.set(true);
    const v = this.employeeForm.getRawValue();
    this.api
      .generateForEmployee({
        employeeId: v.employeeId.trim(),
        performanceCycleId: v.performanceCycleId.trim(),
        target: v.target,
      })
      .subscribe({
        next: (res) => {
          this.genEmployeeBusy.set(false);
          this.toast.show(this.genSuccessMsg(res), 'success');
          this.refreshCounts();
        },
        error: () => {
          this.genEmployeeBusy.set(false);
          this.toast.show(this.langMsg('فشل توليد الذكاء للموظف', 'Employee intelligence generation failed'), 'error');
        },
      });
  }

  runCycleGeneration(): void {
    if (this.cycleForm.invalid || !this.auth.hasPermission(PermissionCodesConst.IntelligenceGenerate)) return;
    this.genCycleBusy.set(true);
    const id = this.cycleForm.controls.performanceCycleId.value.trim();
    this.api.generateForCycle({ performanceCycleId: id }).subscribe({
      next: (res) => {
        this.genCycleBusy.set(false);
        this.toast.show(
          `تم توليد الدورة: رؤى ${res.insightsGenerated} · توصيات ${res.recommendationsGenerated}`,
          'success',
        );
        this.refreshCounts();
      },
      error: () => {
        this.genCycleBusy.set(false);
        this.toast.show('فشل توليد الذكاء للدورة', 'error');
      },
    });
  }

  private refreshCounts(): void {
    forkJoin({
      i: this.api.getInsightsPaged({ page: 1, pageSize: 1 }),
      r: this.api.getRecommendationsPaged({ page: 1, pageSize: 1 }),
    }).subscribe({
      next: ({ i, r }) => {
        this.insightTotal.set(i.totalCount);
        this.recommendationTotal.set(r.totalCount);
      },
    });
  }

  cycleLabel(c: PerformanceCycleDto): string {
    const ar = this.i18n.lang() === 'ar';
    if (ar) {
      return c.nameAr?.trim() ? c.nameAr : c.nameEn;
    }
    return c.nameEn?.trim() ? c.nameEn : c.nameAr;
  }

  private genSuccessMsg(res: { insightsGenerated: number; recommendationsGenerated: number }): string {
    return this.langMsg(
      `تم التوليد: رؤى ${res.insightsGenerated} · توصيات ${res.recommendationsGenerated}`,
      `Generated: ${res.insightsGenerated} insights · ${res.recommendationsGenerated} recommendations`,
    );
  }

  private langMsg(ar: string, en: string): string {
    return this.i18n.lang() === 'ar' ? ar : en;
  }
}
