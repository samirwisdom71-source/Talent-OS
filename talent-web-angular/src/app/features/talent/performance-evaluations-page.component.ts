import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, startWith, switchMap } from 'rxjs/operators';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PerformanceCyclesLookupService } from '../../services/performance-cycles-lookup.service';
import { PerformanceEvaluationsApiService } from '../../services/performance-evaluations-api.service';
import { PERFORMANCE_CYCLE_STATUS_ACTIVE } from '../../shared/constants/performance-cycle-status';
import { LookupItemDto } from '../../shared/models/lookup.models';
import {
  CreatePerformanceEvaluationRequest,
  PerformanceEvaluationDto,
  UpdatePerformanceEvaluationRequest,
} from '../../shared/models/performance.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { SearchableSelectComponent } from '../../shared/ui/searchable-select.component';
import { I18nService } from '../../shared/services/i18n.service';
type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-performance-evaluations-page',
  standalone: true,
  imports: [FormsModule, DecimalPipe, RouterLink, SearchableSelectComponent, TranslatePipe],
  templateUrl: './performance-evaluations-page.component.html',
  styleUrl: './performance-evaluations-page.component.scss',
})
export class PerformanceEvaluationsPageComponent implements OnInit {
  private readonly api = inject(PerformanceEvaluationsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesLookup = inject(PerformanceCyclesLookupService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);

  readonly employees = signal<LookupItemDto[]>([]);
  /** دورات نشطة فقط — للاختيار والإنشاء */
  readonly cycles = signal<LookupItemDto[]>([]);
  /** كل الدورات المحمّلة لعرض الأسماء في الجدول (قد تشمل مغلقة) */
  readonly cycleLabels = signal<LookupItemDto[]>([]);
  readonly rows = signal<PerformanceEvaluationDto[]>([]);
  readonly busy = signal(false);
  readonly openCreate = signal(false);
  readonly openDetails = signal(false);
  readonly selected = signal<PerformanceEvaluationDto | null>(null);

  filterEmployeeId = '';
  filterCycleId = '';
  viewMode: ViewMode = 'cards';
  private readonly employeeSearch$ = new Subject<string>();
  private readonly createEmployeeSearch$ = new Subject<string>();
  private readonly cycleSearch$ = new Subject<string>();

  createModel: CreatePerformanceEvaluationRequest = {
    employeeId: '',
    performanceCycleId: '',
    overallScore: 1,
    managerComments: '',
    employeeComments: '',
    status: 1,
  };

  editModel: UpdatePerformanceEvaluationRequest = {
    overallScore: 1,
    managerComments: '',
    employeeComments: '',
    status: 1,
  };

  readonly createEmployees = signal<LookupItemDto[]>([]);

  ngOnInit(): void {
    this.lookups.getEmployees('', 50).subscribe((r) => {
      this.employees.set(r);
      this.createEmployees.set(r);
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

    this.cycleSearch$
      .pipe(
        startWith(''),
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((term) =>
          this.cyclesLookup.loadLookupItems({
            pageSize: 200,
            search: term.trim() || null,
            status: PERFORMANCE_CYCLE_STATUS_ACTIVE,
          }),
        ),
      )
      .subscribe({
        next: (r) => this.cycles.set(r),
        error: () => this.toast.show('تعذر تحميل دورات الأداء النشطة', 'error'),
      });

    this.employeeSearch$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((term) => this.lookups.getEmployees(term, 50)),
      )
      .subscribe((r) => this.employees.set(r));

    this.createEmployeeSearch$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((term) => this.lookups.getEmployees(term, 50)),
      )
      .subscribe((r) => this.createEmployees.set(r));

    this.load();
  }

  onFilterEmployeeChange(id: string): void {
    this.filterEmployeeId = id;
    this.load();
  }
  onFilterEmployeeSearch(term: string): void {
    this.employeeSearch$.next(term);
  }
  onFilterCycleChange(id: string): void {
    this.filterCycleId = id;
    this.load();
  }
  onCycleSearch(term: string): void {
    this.cycleSearch$.next(term);
  }
  onCreateEmployeeSearch(term: string): void {
    this.createEmployeeSearch$.next(term);
  }

  load(): void {
    this.busy.set(true);
    this.api
      .getPaged({
        page: 1,
        pageSize: 200,
        employeeId: this.filterEmployeeId || null,
        performanceCycleId: this.filterCycleId || null,
      })
      .subscribe({
        next: (r) => {
          this.rows.set([...r.items]);
          this.busy.set(false);
        },
        error: () => {
          this.toast.show('تعذر تحميل التقييمات', 'error');
          this.busy.set(false);
        },
      });
  }

  labelFrom(list: LookupItemDto[], id: string): string {
    return list.find((x) => x.id === id)?.name ?? id;
  }

  cycleDisplayName(id: string): string {
    return this.labelFrom(this.cycleLabels(), id);
  }

  statusLabel(v: number): string {
    if (this.i18n.lang() === 'en') {
      if (v === 1) return 'Draft';
      if (v === 2) return 'Submitted';
      if (v === 3) return 'In review';
      if (v === 4) return 'Approved';
      return `#${v}`;
    }
    if (v === 1) return 'مسودة';
    if (v === 2) return 'مُرسل';
    if (v === 3) return 'قيد المراجعة';
    if (v === 4) return 'معتمد';
    return `#${v}`;
  }

  submitCreate(): void {
    this.api.create(this.createModel).subscribe({
      next: () => {
        this.toast.show('تم إنشاء التقييم', 'success');
        this.openCreate.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر إنشاء التقييم', 'error'),
    });
  }

  openRow(row: PerformanceEvaluationDto): void {
    this.api.getById(row.id).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.editModel = {
          overallScore: full.overallScore,
          managerComments: full.managerComments ?? '',
          employeeComments: full.employeeComments ?? '',
          status: full.status,
        };
        this.openDetails.set(true);
      },
      error: () => this.toast.show('تعذر تحميل تفاصيل التقييم', 'error'),
    });
  }

  saveEdit(): void {
    const row = this.selected();
    if (!row) return;
    this.api.update(row.id, this.editModel).subscribe({
      next: () => {
        this.toast.show('تم تحديث التقييم', 'success');
        this.openDetails.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر تحديث التقييم', 'error'),
    });
  }
}
