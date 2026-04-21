import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  CreatePerformanceCycleRequest,
  PerformanceCycleDto,
  UpdatePerformanceCycleRequest,
} from '../../shared/models/performance.models';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { I18nService } from '../../shared/services/i18n.service';
type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-performance-cycles-page',
  standalone: true,
  imports: [FormsModule, DatePipe, RouterLink],
  templateUrl: './performance-cycles-page.component.html',
  styleUrl: './performance-cycles-page.component.scss',
})
export class PerformanceCyclesPageComponent implements OnInit {
  private readonly api = inject(PerformanceCyclesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  search = '';
  status: number | '' = '';
  viewMode: ViewMode = 'table';
  page = 1;
  readonly pageSize = 20;

  readonly result = signal<PagedResult<PerformanceCycleDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);

  readonly openCreate = signal(false);
  readonly openEdit = signal(false);
  readonly openDetails = signal(false);
  readonly actionBusy = signal(false);
  readonly selected = signal<PerformanceCycleDto | null>(null);

  createModel: CreatePerformanceCycleRequest = { nameAr: '', nameEn: '', startDate: '', endDate: '', description: '' };
  editModel: UpdatePerformanceCycleRequest = { nameAr: '', nameEn: '', startDate: '', endDate: '', description: '' };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.failed.set(false);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search || null,
        status: this.status === '' ? null : Number(this.status),
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.busy.set(false);
        },
        error: () => {
          this.failed.set(true);
          this.result.set(null);
          this.busy.set(false);
        },
      });
  }

  statusLabel(v: number): string {
    return EnumLabels.performanceCycleStatus(this.i18n.lang() as UiLang, v);
  }

  name(row: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? row.nameAr || row.nameEn : row.nameEn || row.nameAr;
  }

  submitCreate(): void {
    if (!this.createModel.nameAr.trim() || !this.createModel.nameEn.trim()) return;
    this.actionBusy.set(true);
    this.api
      .create({
        ...this.createModel,
        nameAr: this.createModel.nameAr.trim(),
        nameEn: this.createModel.nameEn.trim(),
        description: this.createModel.description?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show('تم إنشاء دورة الأداء', 'success');
          this.openCreate.set(false);
          this.actionBusy.set(false);
          this.page = 1;
          this.load();
        },
        error: () => {
          this.toast.show('تعذر إنشاء الدورة', 'error');
          this.actionBusy.set(false);
        },
      });
  }

  openEditModal(row: PerformanceCycleDto): void {
    this.selected.set(row);
    this.editModel = {
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      startDate: row.startDate.slice(0, 10),
      endDate: row.endDate.slice(0, 10),
      description: row.description ?? '',
    };
    this.openEdit.set(true);
  }

  submitEdit(): void {
    const row = this.selected();
    if (!row) return;
    this.actionBusy.set(true);
    this.api
      .update(row.id, {
        ...this.editModel,
        nameAr: this.editModel.nameAr.trim(),
        nameEn: this.editModel.nameEn.trim(),
        description: this.editModel.description?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.toast.show('تم تحديث الدورة', 'success');
          this.openEdit.set(false);
          this.actionBusy.set(false);
          this.load();
        },
        error: () => {
          this.toast.show('تعذر تحديث الدورة', 'error');
          this.actionBusy.set(false);
        },
      });
  }

  activate(row: PerformanceCycleDto): void {
    this.actionBusy.set(true);
    this.api.activate(row.id).subscribe({
      next: () => {
        this.toast.show('تم تفعيل الدورة', 'success');
        this.actionBusy.set(false);
        this.load();
      },
      error: () => {
        this.toast.show('تعذر تفعيل الدورة', 'error');
        this.actionBusy.set(false);
      },
    });
  }

  closeCycle(row: PerformanceCycleDto): void {
    this.actionBusy.set(true);
    this.api.close(row.id).subscribe({
      next: () => {
        this.toast.show('تم إغلاق الدورة', 'success');
        this.actionBusy.set(false);
        this.load();
      },
      error: () => {
        this.toast.show('تعذر إغلاق الدورة', 'error');
        this.actionBusy.set(false);
      },
    });
  }

  showDetails(row: PerformanceCycleDto): void {
    this.actionBusy.set(true);
    this.api.getById(row.id).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.openDetails.set(true);
        this.actionBusy.set(false);
      },
      error: () => {
        this.toast.show('تعذر تحميل التفاصيل', 'error');
        this.actionBusy.set(false);
      },
    });
  }
}
