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
import { PerformanceGoalsApiService } from '../../services/performance-goals-api.service';
import { PERFORMANCE_CYCLE_STATUS_ACTIVE } from '../../shared/constants/performance-cycle-status';
import { LookupItemDto } from '../../shared/models/lookup.models';
import {
  CreatePerformanceGoalRequest,
  PerformanceGoalDto,
  UpdatePerformanceGoalRequest,
} from '../../shared/models/performance.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { SearchableSelectComponent } from '../../shared/ui/searchable-select.component';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-performance-goals-page',
  standalone: true,
  imports: [FormsModule, DecimalPipe, RouterLink, SearchableSelectComponent, TranslatePipe],
  templateUrl: './performance-goals-page.component.html',
  styleUrl: './performance-goals-page.component.scss',
})
export class PerformanceGoalsPageComponent implements OnInit {
  private readonly api = inject(PerformanceGoalsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly cyclesLookup = inject(PerformanceCyclesLookupService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);

  readonly employees = signal<LookupItemDto[]>([]);
  readonly cycles = signal<LookupItemDto[]>([]);
  readonly cycleLabels = signal<LookupItemDto[]>([]);
  readonly rows = signal<PerformanceGoalDto[]>([]);
  readonly selected = signal<PerformanceGoalDto | null>(null);
  readonly busy = signal(false);
  readonly createOpen = signal(false);
  readonly editOpen = signal(false);

  search = '';
  cycleId = '';
  employeeId = '';
  viewMode: ViewMode = 'table';
  readonly createEmployees = signal<LookupItemDto[]>([]);
  private readonly employeeSearch$ = new Subject<string>();
  private readonly createEmployeeSearch$ = new Subject<string>();
  private readonly cycleSearch$ = new Subject<string>();

  createModel: CreatePerformanceGoalRequest = {
    employeeId: '',
    performanceCycleId: '',
    titleAr: '',
    titleEn: '',
    description: '',
    weight: 10,
    targetValue: '',
    status: 1,
  };
  editModel: UpdatePerformanceGoalRequest = { titleAr: '', titleEn: '', description: '', weight: 10, targetValue: '', status: 1 };

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
    this.employeeId = id;
  }
  onFilterEmployeeSearch(term: string): void {
    this.employeeSearch$.next(term);
  }
  onFilterCycleChange(id: string): void {
    this.cycleId = id;
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
        search: this.search || null,
        employeeId: this.employeeId || null,
        performanceCycleId: this.cycleId || null,
      })
      .subscribe({
        next: (r) => {
          this.rows.set([...r.items]);
          this.busy.set(false);
        },
        error: () => {
          this.busy.set(false);
          this.toast.show('تعذر تحميل الأهداف', 'error');
        },
      });
  }

  title(row: PerformanceGoalDto): string {
    return this.i18n.lang() === 'en' ? row.titleEn || row.titleAr : row.titleAr || row.titleEn;
  }
  empName(id: string): string {
    return this.employees().find((x) => x.id === id)?.name ?? id;
  }
  cycleName(id: string): string {
    return this.cycleLabels().find((x) => x.id === id)?.name ?? id;
  }

  statusLabel(v: number): string {
    if (this.i18n.lang() === 'en') {
      if (v === 1) return 'Draft';
      if (v === 2) return 'Active';
      if (v === 3) return 'Completed';
      if (v === 4) return 'Canceled';
      return `#${v}`;
    }
    if (v === 1) return 'مسودة';
    if (v === 2) return 'نشط';
    if (v === 3) return 'مكتمل';
    if (v === 4) return 'ملغى';
    return `#${v}`;
  }

  openDetails(row: PerformanceGoalDto): void {
    this.api.getById(row.id).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.editModel = {
          titleAr: full.titleAr,
          titleEn: full.titleEn,
          description: full.description ?? '',
          weight: full.weight,
          targetValue: full.targetValue ?? '',
          status: full.status,
        };
        this.editOpen.set(true);
      },
      error: () => this.toast.show('تعذر تحميل الهدف', 'error'),
    });
  }

  closeEdit(): void {
    this.editOpen.set(false);
    this.selected.set(null);
  }

  saveSelected(): void {
    const s = this.selected();
    if (!s) return;
    this.api.update(s.id, this.editModel).subscribe({
      next: () => {
        this.toast.show('تم تحديث الهدف', 'success');
        this.editOpen.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر تحديث الهدف', 'error'),
    });
  }

  saveCreate(): void {
    this.api.create(this.createModel).subscribe({
      next: () => {
        this.toast.show('تم إنشاء الهدف', 'success');
        this.createOpen.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر إنشاء الهدف', 'error'),
    });
  }
}
