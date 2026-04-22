import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { OrganizationUnitsApiService } from '../../services/organization-units-api.service';
import { PositionsApiService } from '../../services/positions-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { CreatePositionRequest, PositionDto, UpdatePositionRequest } from '../../shared/models/position.models';
import { OrganizationUnitDto } from '../../shared/models/organization-unit.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-positions-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './positions-page.component.html',
  styleUrl: './positions-page.component.scss',
})
export class PositionsPageComponent implements OnInit {
  private readonly api = inject(PositionsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly orgUnitsApi = inject(OrganizationUnitsApiService);
  private readonly toast = inject(ToastService);

  search = '';
  orgUnitId = '';
  jobGradeId = '';
  page = 1;
  readonly pageSize = 20;
  viewMode: ViewMode = 'table';

  readonly result = signal<PagedResult<PositionDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly orgUnits = signal<LookupItemDto[]>([]);
  readonly jobGrades = signal<LookupItemDto[]>([]);

  readonly createOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly editOpen = signal(false);
  readonly actionBusy = signal(false);
  readonly selected = signal<PositionDto | null>(null);

  createModel: CreatePositionRequest = { titleAr: '', titleEn: '', organizationUnitId: '', jobGradeId: '' };
  editModel: UpdatePositionRequest = { titleAr: '', titleEn: '', organizationUnitId: '', jobGradeId: '' };

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  loadLookups(): void {
    this.orgUnitsApi.getPaged({ page: 1, pageSize: 200 }).subscribe({
      next: (r) => this.orgUnits.set(r.items.map((x) => this.mapOrgUnitLookup(x))),
      error: () => this.toast.show('تعذر تحميل الوحدات التنظيمية', 'error'),
    });
    this.lookups.getJobGrades('', 300).subscribe({
      next: (items) => this.jobGrades.set(items),
      error: () => this.toast.show('تعذر تحميل الدرجات الوظيفية', 'error'),
    });
  }

  private mapOrgUnitLookup(item: OrganizationUnitDto): LookupItemDto {
    const selfName = item.nameAr?.trim() || item.nameEn?.trim() || item.id;
    const parentName = item.parentNameAr?.trim() || item.parentNameEn?.trim() || '';
    return {
      id: item.id,
      name: parentName ? `${parentName} / ${selfName}` : selfName,
    };
  }

  load(): void {
    this.busy.set(true);
    this.failed.set(false);
    this.api
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search || null,
        organizationUnitId: this.orgUnitId || null,
        jobGradeId: this.jobGradeId || null,
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
    this.createModel = {
      titleAr: '',
      titleEn: '',
      organizationUnitId: this.orgUnits()[0]?.id ?? '',
      jobGradeId: this.jobGrades()[0]?.id ?? '',
    };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    const titleAr = this.createModel.titleAr.trim();
    const titleEn = this.createModel.titleEn.trim();
    if (!titleAr || !titleEn || !this.createModel.organizationUnitId || !this.createModel.jobGradeId) {
      this.toast.show('املأ جميع الحقول المطلوبة', 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api.create({ ...this.createModel, titleAr, titleEn }).subscribe({
      next: () => {
        this.toast.show('تم إنشاء المنصب', 'success');
        this.createOpen.set(false);
        this.actionBusy.set(false);
        this.page = 1;
        this.load();
      },
      error: () => {
        this.toast.show('تعذر إنشاء المنصب', 'error');
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
        this.toast.show('تعذر تحميل التفاصيل', 'error');
        this.actionBusy.set(false);
      },
    });
  }

  openEdit(id: string): void {
    this.actionBusy.set(true);
    this.api.getById(id).subscribe({
      next: (item) => {
        this.selected.set(item);
        this.editModel = {
          titleAr: item.titleAr,
          titleEn: item.titleEn,
          organizationUnitId: item.organizationUnitId,
          jobGradeId: item.jobGradeId,
        };
        this.editOpen.set(true);
        this.detailsOpen.set(false);
        this.actionBusy.set(false);
      },
      error: () => {
        this.toast.show('تعذر تحميل بيانات التعديل', 'error');
        this.actionBusy.set(false);
      },
    });
  }

  saveEdit(): void {
    const selected = this.selected();
    if (!selected) return;
    const titleAr = this.editModel.titleAr.trim();
    const titleEn = this.editModel.titleEn.trim();
    if (!titleAr || !titleEn || !this.editModel.organizationUnitId || !this.editModel.jobGradeId) {
      this.toast.show('املأ جميع الحقول المطلوبة', 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api.update(selected.id, { ...this.editModel, titleAr, titleEn }).subscribe({
      next: () => {
        this.toast.show('تم تحديث المنصب', 'success');
        this.editOpen.set(false);
        this.actionBusy.set(false);
        this.load();
      },
      error: () => {
        this.toast.show('تعذر تحديث المنصب', 'error');
        this.actionBusy.set(false);
      },
    });
  }
}
