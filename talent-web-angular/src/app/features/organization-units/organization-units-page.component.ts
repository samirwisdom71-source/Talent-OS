import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { OrganizationUnitsApiService } from '../../services/organization-units-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import {
  CreateOrganizationUnitRequest,
  OrganizationUnitDto,
  UpdateOrganizationUnitRequest,
} from '../../shared/models/organization-unit.models';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-organization-units-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './organization-units-page.component.html',
  styleUrl: './organization-units-page.component.scss',
})
export class OrganizationUnitsPageComponent implements OnInit {
  private readonly api = inject(OrganizationUnitsApiService);
  private readonly lookups = inject(IdentityLookupsApiService);
  private readonly toast = inject(ToastService);

  search = '';
  page = 1;
  readonly pageSize = 20;
  viewMode: ViewMode = 'table';
  readonly result = signal<PagedResult<OrganizationUnitDto> | null>(null);
  readonly busy = signal(false);
  readonly failed = signal(false);
  readonly unitsLookup = signal<LookupItemDto[]>([]);

  readonly createOpen = signal(false);
  readonly detailsOpen = signal(false);
  readonly editOpen = signal(false);
  readonly actionBusy = signal(false);
  readonly selected = signal<OrganizationUnitDto | null>(null);

  createModel: CreateOrganizationUnitRequest = { nameAr: '', nameEn: '', parentId: null };
  editModel: UpdateOrganizationUnitRequest = { nameAr: '', nameEn: '', parentId: null };

  ngOnInit(): void {
    this.loadLookup();
    this.load();
  }

  loadLookup(): void {
    this.lookups.getOrganizationUnits('', 300).subscribe({
      next: (items) => this.unitsLookup.set(items),
      error: () => this.toast.show('تعذر تحميل الوحدات التنظيمية', 'error'),
    });
  }

  load(): void {
    this.busy.set(true);
    this.failed.set(false);
    this.api.getPaged({ page: this.page, pageSize: this.pageSize, search: this.search || null }).subscribe({
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

  parentName(id?: string | null): string {
    if (!id) return '—';
    return this.unitsLookup().find((x) => x.id === id)?.name ?? id;
  }

  openCreate(): void {
    this.createModel = { nameAr: '', nameEn: '', parentId: null };
    this.createOpen.set(true);
  }

  saveCreate(): void {
    const nameAr = this.createModel.nameAr.trim();
    const nameEn = this.createModel.nameEn.trim();
    if (!nameAr || !nameEn) {
      this.toast.show('الاسم العربي والإنجليزي مطلوبان', 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api.create({ nameAr, nameEn, parentId: this.createModel.parentId || null }).subscribe({
      next: () => {
        this.toast.show('تم إنشاء الوحدة التنظيمية', 'success');
        this.createOpen.set(false);
        this.actionBusy.set(false);
        this.page = 1;
        this.loadLookup();
        this.load();
      },
      error: () => {
        this.toast.show('تعذر إنشاء الوحدة التنظيمية', 'error');
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
          nameAr: item.nameAr,
          nameEn: item.nameEn,
          parentId: item.parentId ?? null,
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
    const nameAr = this.editModel.nameAr.trim();
    const nameEn = this.editModel.nameEn.trim();
    if (!nameAr || !nameEn) {
      this.toast.show('الاسم العربي والإنجليزي مطلوبان', 'error');
      return;
    }

    this.actionBusy.set(true);
    this.api
      .update(selected.id, { nameAr, nameEn, parentId: this.editModel.parentId || null })
      .subscribe({
        next: () => {
          this.toast.show('تم تحديث الوحدة التنظيمية', 'success');
          this.editOpen.set(false);
          this.actionBusy.set(false);
          this.loadLookup();
          this.load();
        },
        error: () => {
          this.toast.show('تعذر تحديث الوحدة التنظيمية', 'error');
          this.actionBusy.set(false);
        },
      });
  }
}
