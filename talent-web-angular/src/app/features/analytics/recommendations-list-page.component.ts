import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/auth/auth.service';
import { IntelligenceApiService } from '../../services/intelligence-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { PagedResult } from '../../shared/models/api.types';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { TalentRecommendationDto } from '../../shared/models/intelligence.models';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

const REC_ACTIVE = 1;

@Component({
  selector: 'app-recommendations-list-page',
  standalone: true,
  imports: [DatePipe, RouterLink, TranslatePipe],
  templateUrl: './recommendations-list-page.component.html',
  styleUrl: './recommendations-list-page.component.scss',
})
export class RecommendationsListPageComponent implements OnInit {
  private readonly api = inject(IntelligenceApiService);
  private readonly identity = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<TalentRecommendationDto> | null>(null);
  readonly failed = signal(false);
  private readonly busy = signal<Record<string, boolean>>({});

  private employeeNameById = new Map<string, string>();
  private cycleNameById = new Map<string, string>();

  ngOnInit(): void {
    forkJoin({
      emps: this.identity.getEmployees(undefined, 500),
      cycles: this.cyclesApi.getPaged({ page: 1, pageSize: 200 }),
    }).subscribe({
      next: ({ emps, cycles }) => {
        this.employeeNameById = new Map(emps.map((e) => [e.id.toLowerCase(), e.name]));
        for (const c of cycles.items) {
          this.cycleNameById.set(c.id.toLowerCase(), this.cycleLabel(c));
        }
      },
      error: () => {
        this.employeeNameById = new Map();
        this.cycleNameById = new Map();
      },
    });
    this.load();
  }

  load(): void {
    this.api.getRecommendationsPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (d) => {
        this.data.set(d);
        this.failed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.failed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  statusLabel(v: number): string {
    return EnumLabels.recommendationStatus(this.lang(), v);
  }

  employeeName(id: string | null | undefined): string {
    if (!id) return '—';
    return this.employeeNameById.get(id.toLowerCase()) ?? '—';
  }

  cycleName(id: string | null | undefined): string {
    if (!id) return '—';
    return this.cycleNameById.get(id.toLowerCase()) ?? '—';
  }

  private cycleLabel(c: PerformanceCycleDto): string {
    const ar = this.i18n.lang() === 'ar';
    if (ar) {
      return c.nameAr?.trim() ? c.nameAr : c.nameEn;
    }
    return c.nameEn?.trim() ? c.nameEn : c.nameAr;
  }

  isRowBusy(id: string): boolean {
    return this.busy()[id] ?? false;
  }

  dismiss(row: TalentRecommendationDto): void {
    if (row.status !== REC_ACTIVE) return;
    this.setBusy(row.id, true);
    this.api.dismissRecommendation(row.id).subscribe({
      next: () => {
        this.setBusy(row.id, false);
        this.toast.show(
          this.i18n.lang() === 'ar' ? 'تم تجاهل التوصية' : 'Recommendation dismissed',
          'success',
        );
        this.load();
      },
      error: () => {
        this.setBusy(row.id, false);
        this.toast.show(this.i18n.lang() === 'ar' ? 'تعذر التجاهل' : 'Could not dismiss', 'error');
      },
    });
  }

  accept(row: TalentRecommendationDto): void {
    if (row.status !== REC_ACTIVE) return;
    this.setBusy(row.id, true);
    this.api.acceptRecommendation(row.id).subscribe({
      next: () => {
        this.setBusy(row.id, false);
        this.toast.show(this.i18n.lang() === 'ar' ? 'تم قبول التوصية' : 'Recommendation accepted', 'success');
        this.load();
      },
      error: () => {
        this.setBusy(row.id, false);
        this.toast.show(this.i18n.lang() === 'ar' ? 'تعذر القبول' : 'Could not accept', 'error');
      },
    });
  }

  private setBusy(id: string, v: boolean): void {
    this.busy.update((m) => ({ ...m, [id]: v }));
  }
}
