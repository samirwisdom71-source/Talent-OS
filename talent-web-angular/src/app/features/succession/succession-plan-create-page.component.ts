import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { CreateSuccessionPlanRequest, CriticalPositionDto } from '../../shared/models/succession.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-succession-plan-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe],
  templateUrl: './succession-plan-create-page.component.html',
  styleUrl: '../development/development-create-page.component.scss',
})
export class SuccessionPlanCreatePageComponent implements OnInit {
  private readonly api = inject(SuccessionApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  readonly cycles = signal<readonly PerformanceCycleDto[]>([]);
  readonly positions = signal<readonly CriticalPositionDto[]>([]);
  private readonly positionLookup = signal(new Map<string, string>());

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
      posLookup: this.identityLookups.getPositions(undefined, 400),
    }).subscribe({
      next: ({ cy, cp, posLookup }) => {
        this.cycles.set(cy.items);
        this.positions.set(cp.items);
        this.positionLookup.set(new Map(posLookup.map((x) => [x.id, x.name])));
      },
      error: () => {
        this.cycles.set([]);
        this.positions.set([]);
        this.positionLookup.set(new Map());
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  positionName(positionId: string): string {
    return this.positionLookup().get(positionId) ?? '';
  }

  criticalPositionOptionLabel(p: CriticalPositionDto): string {
    const name = this.positionName(p.positionId);
    const crit = EnumLabels.criticalityLevel(this.lang(), p.criticalityLevel);
    return name ? `${name} · ${crit}` : crit;
  }

  cycleLabel(c: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? c.nameAr || c.nameEn : c.nameEn || c.nameAr;
  }

  save(): void {
    this.busy.set(true);
    this.api.createPlan(this.model).subscribe({
      next: (p) => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('تم إنشاء الخطة'), 'success');
        void this.router.navigate(['/succession/plans', p.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('successionPlans.toast.createFailed'), 'error');
      },
    });
  }
}
