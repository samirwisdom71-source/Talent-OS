import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { JobGradesApiService } from '../../services/job-grades-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { CreateJobGradeRequest, JobGradeDto, UpdateJobGradeRequest } from '../../shared/models/job-grade.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-job-grades-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './job-grades-page.component.html',
  styleUrl: './job-grades-page.component.scss',
})
export class JobGradesPageComponent implements OnInit {
  private readonly api = inject(JobGradesApiService);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);

  search = '';
  level: number | '' = '';
  page = 1;
  readonly pageSize = 20;
  viewMode: ViewMode = 'table';

  readonly result = signal<PagedResult<JobGradeDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly createOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly editOpen = signal(false);
  readonly actionBusy = signal(false);
  readonly selected = signal<JobGradeDto | null>(null);

  createModel: CreateJobGradeRequest = { name: '', level: 1 };
  editModel: UpdateJobGradeRequest = { name: '', level: 1 };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.failed.set(false);
    this.api
      .getPaged({ page: this.page, pageSize: this.pageSize, search: this.search || null, level: this.level === '' ? null : this.level })
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

  prevPage(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }

  nextPage(): void {
    const r = this.result();
    if (!r || !r.hasNextPage) return;
    this.page += 1;
    this.load();
  }

  openCreate(): void {
    this.createModel = { name: '', level: 1 };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    const name = this.createModel.name.trim();
    if (!name || this.createModel.level <= 0) {
      this.toast.show(this.i18n.t('أدخل اسم صحيح ومستوى أكبر من 0'), 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api.create({ name, level: this.createModel.level }).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('تم إنشاء الدرجة الوظيفية'), 'success');
        this.createOpen.set(false);
        this.actionBusy.set(false);
        this.page = 1;
        this.load();
      },
      error: () => {
        this.toast.show(this.i18n.t('تعذر إنشاء الدرجة الوظيفية'), 'error');
        this.actionBusy.set(false);
      },
    });
  }

  openDetails(id: string): void {
    this.actionBusy.set(true);
    this.api.getById(id).subscribe({
      next: (item) => {
        this.selected.set(item);
        this.detailsOpen.set(true);
        this.actionBusy.set(false);
      },
      error: () => {
        this.toast.show(this.i18n.t('تعذر تحميل التفاصيل'), 'error');
        this.actionBusy.set(false);
      },
    });
  }

  openEdit(id: string): void {
    this.actionBusy.set(true);
    this.api.getById(id).subscribe({
      next: (item) => {
        this.selected.set(item);
        this.editModel = { name: item.name, level: item.level };
        this.editOpen.set(true);
        this.detailsOpen.set(false);
        this.actionBusy.set(false);
      },
      error: () => {
        this.toast.show(this.i18n.t('تعذر تحميل بيانات التعديل'), 'error');
        this.actionBusy.set(false);
      },
    });
  }

  saveEdit(): void {
    const selected = this.selected();
    if (!selected) return;
    const name = this.editModel.name.trim();
    if (!name || this.editModel.level <= 0) {
      this.toast.show(this.i18n.t('أدخل اسم صحيح ومستوى أكبر من 0'), 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api.update(selected.id, { name, level: this.editModel.level }).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('تم تحديث الدرجة الوظيفية'), 'success');
        this.editOpen.set(false);
        this.actionBusy.set(false);
        this.load();
      },
      error: () => {
        this.toast.show(this.i18n.t('تعذر تحديث الدرجة الوظيفية'), 'error');
        this.actionBusy.set(false);
      },
    });
  }
}
