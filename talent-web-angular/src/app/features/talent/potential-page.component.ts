import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PerformanceCyclesLookupService } from '../../services/performance-cycles-lookup.service';
import { PotentialAssessmentsApiService } from '../../services/potential-assessments-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  CreatePotentialAssessmentRequest,
  PotentialAssessmentDto,
  UpdatePotentialAssessmentRequest,
} from '../../shared/models/potential.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PERFORMANCE_CYCLE_STATUS_ACTIVE } from '../../shared/constants/performance-cycle-status';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type PotentialViewMode = 'table' | 'cards';

@Component({
  selector: 'app-potential-page',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, TranslatePipe],
  templateUrl: './potential-page.component.html',
  styleUrl: './potential-page.component.scss',
})
export class PotentialPageComponent implements OnInit {
  private readonly api = inject(PotentialAssessmentsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesLookup = inject(PerformanceCyclesLookupService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly data = signal<PagedResult<PotentialAssessmentDto> | null>(null);
  readonly busy = signal(false);
  readonly employees = signal<LookupItemDto[]>([]);
  /** دورات نشطة — للفلتر والإنشاء */
  readonly cycles = signal<LookupItemDto[]>([]);
  /** أسماء كل الدورات المحمّلة — لعرض الصفوف والتفاصيل */
  readonly cycleLabels = signal<LookupItemDto[]>([]);
  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly selected = signal<PotentialAssessmentDto | null>(null);

  readonly detailsOpen = signal(false);
  readonly detailsBusy = signal(false);
  readonly detailRecord = signal<PotentialAssessmentDto | null>(null);

  filterEmployeeId = '';
  filterCycleId = '';
  filterStatus = '';
  filterPotentialLevel = '';
  viewMode: PotentialViewMode = 'table';

  page = 1;
  readonly pageSize = 20;

  createModel: CreatePotentialAssessmentRequest = {
    employeeId: '',
    performanceCycleId: '',
    assessedByEmployeeId: '',
    agilityScore: 1,
    leadershipScore: 1,
    growthScore: 1,
    mobilityScore: 1,
    comments: '',
    status: 1,
    factors: [],
  };

  editModel: UpdatePotentialAssessmentRequest = {
    assessedByEmployeeId: '',
    agilityScore: 1,
    leadershipScore: 1,
    growthScore: 1,
    mobilityScore: 1,
    comments: '',
    status: 1,
    factors: [],
  };

  ngOnInit(): void {
    this.lookups.getEmployees('', 200).subscribe({
      next: (r) => this.employees.set(r),
      error: () => this.toast.show('تعذر تحميل قائمة الموظفين', 'error'),
    });
    this.cyclesApi.getPaged({ page: 1, pageSize: 200 }).subscribe({
      next: (p) =>
        this.cycleLabels.set(
          p.items.map((c) => ({
            id: c.id,
            name:
              this.i18n.lang() === 'en'
                ? c.nameEn?.trim() || c.nameAr?.trim() || c.id
                : c.nameAr?.trim() || c.nameEn?.trim() || c.id,
          })),
        ),
      error: () => this.toast.show('تعذر تحميل أسماء دورات الأداء', 'error'),
    });

    this.cyclesLookup
      .loadLookupItems({ pageSize: 200, status: PERFORMANCE_CYCLE_STATUS_ACTIVE })
      .subscribe({
        next: (r) => this.cycles.set(r),
        error: () => this.toast.show('تعذر تحميل دورات الأداء النشطة', 'error'),
      });
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        employeeId: this.filterEmployeeId || null,
        performanceCycleId: this.filterCycleId || null,
        status: this.filterStatus ? Number(this.filterStatus) : null,
        potentialLevel: this.filterPotentialLevel ? Number(this.filterPotentialLevel) : null,
      })
      .subscribe({
        next: (d) => {
          this.data.set(d);
          this.busy.set(false);
        },
        error: () => {
          this.data.set(null);
          this.busy.set(false);
          this.toast.show('تعذر تحميل تقييمات الإمكانات', 'error');
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  prevPage(): void {
    if (!this.data()?.hasPreviousPage) return;
    this.page--;
    this.load();
  }

  nextPage(): void {
    if (!this.data()?.hasNextPage) return;
    this.page++;
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  potentialLevel(v: number): string {
    return EnumLabels.potentialLevel(this.lang(), v);
  }

  assessmentStatus(v: number): string {
    return EnumLabels.potentialAssessmentStatus(this.lang(), v);
  }

  nameFrom(list: LookupItemDto[], id: string): string {
    return list.find((x) => x.id === id)?.name ?? id;
  }

  openDetails(id: string): void {
    this.detailsOpen.set(true);
    this.detailsBusy.set(true);
    this.detailRecord.set(null);
    this.api.getById(id).subscribe({
      next: (d) => {
        this.detailRecord.set(d);
        this.detailsBusy.set(false);
      },
      error: () => {
        this.toast.show('تعذر تحميل التفاصيل من الخادم', 'error');
        this.detailsBusy.set(false);
        this.detailsOpen.set(false);
      },
    });
  }

  closeDetails(): void {
    this.detailsOpen.set(false);
    this.detailRecord.set(null);
    this.detailsBusy.set(false);
  }

  editFromDetails(): void {
    const d = this.detailRecord();
    if (!d) return;
    this.closeDetails();
    this.openEdit(d);
  }

  openCreate(): void {
    this.createModel = {
      employeeId: '',
      performanceCycleId: '',
      assessedByEmployeeId: '',
      agilityScore: 1,
      leadershipScore: 1,
      growthScore: 1,
      mobilityScore: 1,
      comments: '',
      status: 1,
      factors: [],
    };
    this.createOpen.set(true);
  }

  submitCreate(): void {
    if (!this.createModel.employeeId || !this.createModel.performanceCycleId || !this.createModel.assessedByEmployeeId) {
      this.toast.show('املأ الحقول المطلوبة', 'error');
      return;
    }
    this.api.create(this.createModel).subscribe({
      next: () => {
        this.toast.show('تم إنشاء التقييم', 'success');
        this.createOpen.set(false);
        this.applyFilters();
      },
      error: () => this.toast.show('تعذر إنشاء التقييم', 'error'),
    });
  }

  openEdit(row: PotentialAssessmentDto): void {
    this.selected.set(row);
    this.editModel = {
      assessedByEmployeeId: row.assessedByEmployeeId,
      agilityScore: row.agilityScore,
      leadershipScore: row.leadershipScore,
      growthScore: row.growthScore,
      mobilityScore: row.mobilityScore,
      comments: row.comments ?? '',
      status: row.status,
      factors: row.factors.map((f) => ({
        factorName: f.factorName,
        score: f.score,
        weight: f.weight,
        notes: f.notes ?? null,
      })),
    };
    this.editOpen.set(true);
  }

  submitEdit(): void {
    const row = this.selected();
    if (!row) return;
    if (!this.editModel.assessedByEmployeeId) {
      this.toast.show('حدد المقيم', 'error');
      return;
    }
    this.api.update(row.id, this.editModel).subscribe({
      next: () => {
        this.toast.show('تم تحديث التقييم', 'success');
        this.editOpen.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر تحديث التقييم', 'error'),
    });
  }

  meterPct(score: number): number {
    const n = Number(score);
    if (!Number.isFinite(n)) return 0;
    return Math.min(100, Math.max(0, n));
  }
}
