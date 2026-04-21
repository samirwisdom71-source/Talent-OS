import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { CompetenciesApiService } from '../../services/competencies-api.service';
import { CompetencyLevelsApiService } from '../../services/competency-levels-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { JobCompetencyRequirementsApiService } from '../../services/job-competency-requirements-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  CreateJobCompetencyRequirementRequest,
  JobCompetencyRequirementDto,
  UpdateJobCompetencyRequirementRequest,
} from '../../shared/models/job-competency-requirement.models';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { I18nService } from '../../shared/services/i18n.service';

type JcrViewMode = 'table' | 'cards';

@Component({
  selector: 'app-job-competency-requirements-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './job-competency-requirements-page.component.html',
  styleUrl: './job-competency-requirements-page.component.scss',
})
export class JobCompetencyRequirementsPageComponent implements OnInit {
  private readonly api = inject(JobCompetencyRequirementsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly competenciesApi = inject(CompetenciesApiService);
  private readonly competencyLevelsApi = inject(CompetencyLevelsApiService);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);

  readonly result = signal<PagedResult<JobCompetencyRequirementDto> | null>(null);
  readonly positions = signal<LookupItemDto[]>([]);
  readonly competencies = signal<LookupItemDto[]>([]);
  readonly competencyLevels = signal<LookupItemDto[]>([]);
  readonly busy = signal(false);
  readonly createOpen = signal(false);
  readonly editOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly selected = signal<JobCompetencyRequirementDto | null>(null);

  page = 1;
  readonly pageSize = 20;
  filterPositionId = '';
  filterCompetencyId = '';
  viewMode: JcrViewMode = 'table';

  createModel: CreateJobCompetencyRequirementRequest = {
    positionId: '',
    competencyId: '',
    requiredLevelId: '',
  };

  editModel: UpdateJobCompetencyRequirementRequest = {
    positionId: '',
    competencyId: '',
    requiredLevelId: '',
  };

  ngOnInit(): void {
    this.loadPositions();
    this.loadCompetencies();
    this.loadCompetencyLevels();
    this.load();
  }

  /** GET /api/identity/lookups/positions */
  private loadPositions(): void {
    this.lookups.getPositions('', 200).subscribe({
      next: (r) => this.positions.set(r),
      error: () => this.toast.show('تعذر تحميل قائمة المناصب', 'error'),
    });
  }

  /** GET /api/competencies/lookup */
  private loadCompetencies(): void {
    this.competenciesApi.getLookup({ take: 200, lang: this.i18n.lang() }).subscribe({
      next: (r) => this.competencies.set(r),
      error: () => this.toast.show('تعذر تحميل قائمة الكفاءات', 'error'),
    });
  }

  /** GET /api/competency-levels/lookup */
  private loadCompetencyLevels(): void {
    this.competencyLevelsApi.getLookup({ take: 200 }).subscribe({
      next: (r) => this.competencyLevels.set(r),
      error: () => this.toast.show('تعذر تحميل مستويات الكفاءة', 'error'),
    });
  }

  load(): void {
    this.busy.set(true);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        positionId: this.filterPositionId || null,
        competencyId: this.filterCompetencyId || null,
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.busy.set(false);
        },
        error: () => {
          this.toast.show('تعذر تحميل متطلبات الكفاءات', 'error');
          this.busy.set(false);
        },
      });
  }

  prevPage(): void {
    if (!this.result()?.hasPreviousPage) return;
    this.page--;
    this.load();
  }

  nextPage(): void {
    if (!this.result()?.hasNextPage) return;
    this.page++;
    this.load();
  }

  labelFrom(list: LookupItemDto[], id: string): string {
    return list.find((x) => x.id === id)?.name ?? id;
  }

  openCreate(): void {
    this.createModel = { positionId: '', competencyId: '', requiredLevelId: '' };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    if (!this.createModel.positionId || !this.createModel.competencyId || !this.createModel.requiredLevelId) {
      this.toast.show('املأ كل الحقول المطلوبة', 'error');
      return;
    }
    this.api.create(this.createModel).subscribe({
      next: () => {
        this.toast.show('تم إنشاء المتطلب', 'success');
        this.createOpen.set(false);
        this.page = 1;
        this.load();
      },
      error: () => this.toast.show('تعذر إنشاء المتطلب', 'error'),
    });
  }

  openDetails(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.detailsOpen.set(true);
      },
      error: () => this.toast.show('تعذر تحميل التفاصيل', 'error'),
    });
  }

  openEdit(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.selected.set(r);
        this.editModel = {
          positionId: r.positionId,
          competencyId: r.competencyId,
          requiredLevelId: r.requiredLevelId,
        };
        this.editOpen.set(true);
      },
      error: () => this.toast.show('تعذر تحميل بيانات التعديل', 'error'),
    });
  }

  saveEdit(): void {
    const row = this.selected();
    if (!row) return;
    if (!this.editModel.positionId || !this.editModel.competencyId || !this.editModel.requiredLevelId) {
      this.toast.show('املأ كل الحقول المطلوبة', 'error');
      return;
    }
    this.api.update(row.id, this.editModel).subscribe({
      next: () => {
        this.toast.show('تم تحديث المتطلب', 'success');
        this.editOpen.set(false);
        this.load();
      },
      error: () => this.toast.show('تعذر تحديث المتطلب', 'error'),
    });
  }
}
