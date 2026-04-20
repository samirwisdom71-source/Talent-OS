import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { CompetencyCategoriesApiService } from '../../services/competency-categories-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  CompetencyCategoryDto,
  CreateCompetencyCategoryRequest,
  UpdateCompetencyCategoryRequest,
} from '../../shared/models/competency-category.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-competency-categories-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './competency-categories-page.component.html',
  styleUrl: './competency-categories-page.component.scss',
})
export class CompetencyCategoriesPageComponent implements OnInit {
  private readonly api = inject(CompetencyCategoriesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  search = '';
  page = 1;
  readonly pageSize = 20;
  readonly viewMode = signal<ViewMode>('table');

  readonly result = signal<PagedResult<CompetencyCategoryDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);

  readonly createOpen = signal(false);
  readonly createBusy = signal(false);
  createModel: CreateCompetencyCategoryRequest = {
    nameAr: '',
    nameEn: '',
    description: '',
  };

  readonly detailsOpen = signal(false);
  readonly detailsBusy = signal(false);
  readonly detailsItem = signal<CompetencyCategoryDto | null>(null);

  readonly editOpen = signal(false);
  readonly editBusy = signal(false);
  readonly editId = signal<string | null>(null);
  editModel: UpdateCompetencyCategoryRequest = {
    nameAr: '',
    nameEn: '',
    description: '',
  };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.failed.set(false);
    this.busy.set(true);
    this.api.getPaged({ page: this.page, pageSize: this.pageSize, search: this.search || null }).subscribe({
      next: (r) => {
        this.result.set(r);
        this.busy.set(false);
      },
      error: () => {
        this.result.set(null);
        this.failed.set(true);
        this.busy.set(false);
      },
    });
  }

  nextPage(): void {
    const r = this.result();
    if (!r || !r.hasNextPage) return;
    this.page += 1;
    this.load();
  }

  prevPage(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  displayName(row: Pick<CompetencyCategoryDto, 'nameAr' | 'nameEn'>): string {
    return this.i18n.lang() === 'ar'
      ? row.nameAr?.trim() || row.nameEn?.trim() || ''
      : row.nameEn?.trim() || row.nameAr?.trim() || '';
  }

  openCreate(): void {
    this.createModel = { nameAr: '', nameEn: '', description: '' };
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (this.createBusy()) return;
    this.createOpen.set(false);
  }

  saveCreate(): void {
    const nameAr = this.createModel.nameAr.trim();
    const nameEn = this.createModel.nameEn.trim();
    if (!nameAr || !nameEn) {
      this.toast.show(this.i18n.t('competencyCategories.toast.namesRequired'), 'error');
      return;
    }
    this.createBusy.set(true);
    const body: CreateCompetencyCategoryRequest = {
      nameAr,
      nameEn,
      description: this.createModel.description?.trim() ? this.createModel.description.trim() : null,
    };
    this.api.create(body).subscribe({
      next: () => {
        this.createBusy.set(false);
        this.createOpen.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.created'), 'success');
        this.page = 1;
        this.load();
      },
      error: () => {
        this.createBusy.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.createFailed'), 'error');
      },
    });
  }

  openDetails(id: string): void {
    this.detailsBusy.set(true);
    this.detailsOpen.set(true);
    this.api.getById(id).subscribe({
      next: (row) => {
        this.detailsItem.set(row);
        this.detailsBusy.set(false);
      },
      error: () => {
        this.detailsBusy.set(false);
        this.detailsOpen.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.loadDetailsFailed'), 'error');
      },
    });
  }

  closeDetails(): void {
    if (this.detailsBusy()) return;
    this.detailsOpen.set(false);
    this.detailsItem.set(null);
  }

  openEdit(id: string): void {
    this.detailsOpen.set(false);
    this.editOpen.set(true);
    this.editBusy.set(true);
    this.editId.set(id);
    this.api.getById(id).subscribe({
      next: (row) => {
        this.editModel = {
          nameAr: row.nameAr,
          nameEn: row.nameEn,
          description: row.description ?? '',
        };
        this.editBusy.set(false);
      },
      error: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.loadEditFailed'), 'error');
      },
    });
  }

  closeEdit(): void {
    if (this.editBusy()) return;
    this.editOpen.set(false);
    this.editId.set(null);
  }

  saveEdit(): void {
    const id = this.editId();
    if (!id) return;
    const nameAr = this.editModel.nameAr.trim();
    const nameEn = this.editModel.nameEn.trim();
    if (!nameAr || !nameEn) {
      this.toast.show(this.i18n.t('competencyCategories.toast.namesRequired'), 'error');
      return;
    }
    this.editBusy.set(true);
    const body: UpdateCompetencyCategoryRequest = {
      nameAr,
      nameEn,
      description: this.editModel.description?.trim() ? this.editModel.description.trim() : null,
    };
    this.api.update(id, body).subscribe({
      next: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.updated'), 'success');
        this.load();
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show(this.i18n.t('competencyCategories.toast.updateFailed'), 'error');
      },
    });
  }
}
