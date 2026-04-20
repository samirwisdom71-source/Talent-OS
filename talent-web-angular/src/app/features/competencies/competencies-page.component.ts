import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { CompetenciesApiService } from '../../services/competencies-api.service';
import { CompetencyCategoriesApiService } from '../../services/competency-categories-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { CompetencyCategoryDto } from '../../shared/models/competency-category.models';
import {
  CompetencyDto,
  CreateCompetencyRequest,
  UpdateCompetencyRequest,
} from '../../shared/models/competency.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-competencies-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './competencies-page.component.html',
  styleUrl: './competencies-page.component.scss',
})
export class CompetenciesPageComponent implements OnInit {
  private readonly api = inject(CompetenciesApiService);
  private readonly categoriesApi = inject(CompetencyCategoriesApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  search = '';
  page = 1;
  readonly pageSize = 20;
  readonly viewMode = signal<ViewMode>('table');

  readonly result = signal<PagedResult<CompetencyDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);
  readonly categories = signal<CompetencyCategoryDto[]>([]);

  readonly createOpen = signal(false);
  readonly createBusy = signal(false);
  createModel: CreateCompetencyRequest = {
    code: '',
    nameAr: '',
    nameEn: '',
    description: '',
    competencyCategoryId: '',
  };

  readonly detailsOpen = signal(false);
  readonly detailsBusy = signal(false);
  readonly detailsItem = signal<CompetencyDto | null>(null);

  readonly editOpen = signal(false);
  readonly editBusy = signal(false);
  readonly editId = signal<string | null>(null);
  editModel: UpdateCompetencyRequest = {
    code: '',
    nameAr: '',
    nameEn: '',
    description: '',
    competencyCategoryId: '',
  };

  ngOnInit(): void {
    this.loadCategories();
    this.load();
  }

  loadCategories(): void {
    this.categoriesApi.getPaged({ page: 1, pageSize: 200 }).subscribe({
      next: (r) => this.categories.set([...r.items]),
      error: () => this.toast.show(this.i18n.t('competencies.toast.loadCategoriesFailed'), 'error'),
    });
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

  displayName(row: Pick<CompetencyDto, 'nameAr' | 'nameEn'>): string {
    return this.i18n.lang() === 'ar'
      ? row.nameAr?.trim() || row.nameEn?.trim() || ''
      : row.nameEn?.trim() || row.nameAr?.trim() || '';
  }

  categoryLabel(c: CompetencyCategoryDto): string {
    return this.i18n.lang() === 'ar'
      ? c.nameAr?.trim() || c.nameEn?.trim() || ''
      : c.nameEn?.trim() || c.nameAr?.trim() || '';
  }

  categoryDisplayName(categoryId: string): string {
    const c = this.categories().find((x) => x.id === categoryId);
    if (!c) return '—';
    return this.categoryLabel(c);
  }

  openCreate(): void {
    this.createModel = {
      code: '',
      nameAr: '',
      nameEn: '',
      description: '',
      competencyCategoryId: this.categories()[0]?.id ?? '',
    };
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (this.createBusy()) return;
    this.createOpen.set(false);
  }

  saveCreate(): void {
    const code = this.createModel.code.trim();
    const nameAr = this.createModel.nameAr.trim();
    const nameEn = this.createModel.nameEn.trim();
    const cat = this.createModel.competencyCategoryId?.trim();
    if (!code || !nameAr || !nameEn || !cat) {
      this.toast.show(this.i18n.t('competencies.toast.requiredFields'), 'error');
      return;
    }
    this.createBusy.set(true);
    const body: CreateCompetencyRequest = {
      code,
      nameAr,
      nameEn,
      description: this.createModel.description?.trim() ? this.createModel.description.trim() : null,
      competencyCategoryId: cat,
    };
    this.api.create(body).subscribe({
      next: () => {
        this.createBusy.set(false);
        this.createOpen.set(false);
        this.toast.show(this.i18n.t('competencies.toast.created'), 'success');
        this.page = 1;
        this.load();
      },
      error: () => {
        this.createBusy.set(false);
        this.toast.show(this.i18n.t('competencies.toast.createFailed'), 'error');
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
        this.toast.show(this.i18n.t('competencies.toast.loadDetailsFailed'), 'error');
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
          code: row.code,
          nameAr: row.nameAr,
          nameEn: row.nameEn,
          description: row.description ?? '',
          competencyCategoryId: row.competencyCategoryId,
        };
        this.editBusy.set(false);
      },
      error: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('competencies.toast.loadEditFailed'), 'error');
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
    const code = this.editModel.code.trim();
    const nameAr = this.editModel.nameAr.trim();
    const nameEn = this.editModel.nameEn.trim();
    const cat = this.editModel.competencyCategoryId?.trim();
    if (!code || !nameAr || !nameEn || !cat) {
      this.toast.show(this.i18n.t('competencies.toast.requiredFields'), 'error');
      return;
    }
    this.editBusy.set(true);
    const body: UpdateCompetencyRequest = {
      code,
      nameAr,
      nameEn,
      description: this.editModel.description?.trim() ? this.editModel.description.trim() : null,
      competencyCategoryId: cat,
    };
    this.api.update(id, body).subscribe({
      next: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('competencies.toast.updated'), 'success');
        this.load();
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show(this.i18n.t('competencies.toast.updateFailed'), 'error');
      },
    });
  }
}
