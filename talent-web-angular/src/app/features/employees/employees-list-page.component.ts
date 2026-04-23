import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { EmployeesApiService } from '../../services/employees-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { OrganizationUnitsApiService } from '../../services/organization-units-api.service';
import { PositionsApiService } from '../../services/positions-api.service';
import { CreateEmployeeRequest, EmployeeListItemDto, UpdateEmployeeRequest } from '../../shared/models/employee.models';
import { PagedResult } from '../../shared/models/api.types';
import { OrganizationUnitDto } from '../../shared/models/organization-unit.models';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PositionDto } from '../../shared/models/position.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-employees-list-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe, LookupSearchComboComponent],
  templateUrl: './employees-list-page.component.html',
  styleUrl: './employees-list-page.component.scss',
})
export class EmployeesListPageComponent implements OnInit {
  private readonly api = inject(EmployeesApiService);
  private readonly lookupApi = inject(IdentityLookupsApiService);
  private readonly orgUnitsApi = inject(OrganizationUnitsApiService);
  private readonly positionsApi = inject(PositionsApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  search = '';
  page = 1;
  readonly pageSize = 20;
  readonly viewMode = signal<ViewMode>('table');

  readonly result = signal<PagedResult<EmployeeListItemDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);
  readonly createOpen = signal(false);
  readonly createBusy = signal(false);
  readonly organizations = signal<LookupItemDto[]>([]);
  readonly positions = signal<LookupItemDto[]>([]);
  readonly organizationsCatalog = signal<OrganizationUnitDto[]>([]);
  readonly positionsCatalog = signal<PositionDto[]>([]);
  readonly selectedOrgUnitName = computed(() => this.lookupName(this.organizations(), this.createModel.organizationUnitId));
  readonly selectedPositionName = computed(() => this.lookupName(this.positions(), this.createModel.positionId));
  readonly detailsOpen = signal(false);
  readonly detailsBusy = signal(false);
  readonly detailsItem = signal<EmployeeListItemDto | null>(null);
  readonly editOpen = signal(false);
  readonly editBusy = signal(false);
  readonly editId = signal<string | null>(null);

  createModel: CreateEmployeeRequest = {
    employeeNumber: '',
    fullNameAr: '',
    fullNameEn: '',
    email: '',
    organizationUnitId: '',
    positionId: '',
  };
  editModel: UpdateEmployeeRequest = {
    employeeNumber: '',
    fullNameAr: '',
    fullNameEn: '',
    email: '',
    organizationUnitId: '',
    positionId: '',
  };

  ngOnInit(): void {
    this.load();
    this.loadLookups();
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

  loadLookups(): void {
    this.lookupApi.getOrganizationUnits('', 200).subscribe({
      next: (rows) => this.organizations.set(rows),
      error: () => this.toast.show(this.i18n.t('employees.toast.loadOrganizationsFailed'), 'error'),
    });
    this.lookupApi.getPositions('', 200).subscribe({
      next: (rows) => this.positions.set(rows),
      error: () => this.toast.show(this.i18n.t('employees.toast.loadPositionsFailed'), 'error'),
    });
    this.orgUnitsApi.getPaged({ page: 1, pageSize: 200 }).subscribe({
      next: (r) => this.organizationsCatalog.set([...r.items]),
      error: () => {},
    });
    this.positionsApi.getPaged({ page: 1, pageSize: 200 }).subscribe({
      next: (r) => this.positionsCatalog.set([...r.items]),
      error: () => {},
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

  openCreate(): void {
    this.createModel = {
      employeeNumber: '',
      fullNameAr: '',
      fullNameEn: '',
      email: '',
      organizationUnitId: '',
      positionId: '',
    };
    this.createOpen.set(true);
  }

  onCreateOrganizationUnitChange(id: string): void {
    this.createModel.organizationUnitId = id;
    this.createModel.positionId = '';
  }

  onEditOrganizationUnitChange(id: string): void {
    this.editModel.organizationUnitId = id;
    this.editModel.positionId = '';
  }

  closeCreate(): void {
    if (this.createBusy()) return;
    this.createOpen.set(false);
  }

  saveCreate(): void {
    const body: CreateEmployeeRequest = {
      employeeNumber: this.createModel.employeeNumber.trim(),
      fullNameAr: this.createModel.fullNameAr.trim(),
      fullNameEn: this.createModel.fullNameEn.trim(),
      email: this.createModel.email.trim(),
      organizationUnitId: this.createModel.organizationUnitId.trim(),
      positionId: this.createModel.positionId.trim(),
    };

    if (
      !body.employeeNumber ||
      !body.fullNameAr ||
      !body.fullNameEn ||
      !body.email ||
      !body.organizationUnitId ||
      !body.positionId
    ) {
      this.toast.show(this.i18n.t('employees.toast.createRequired'), 'error');
      return;
    }

    this.createBusy.set(true);
    this.api.create(body).subscribe({
      next: () => {
        this.createBusy.set(false);
        this.createOpen.set(false);
        this.toast.show(this.i18n.t('employees.toast.created'), 'success');
        this.page = 1;
        this.load();
      },
      error: () => {
        this.createBusy.set(false);
        this.toast.show(this.i18n.t('employees.toast.createFailed'), 'error');
      },
    });
  }

  displayName(row: EmployeeListItemDto): string {
    return this.i18n.lang() === 'ar' ? row.fullNameAr || row.fullNameEn : row.fullNameEn || row.fullNameAr;
  }

  organizationName(row: Pick<EmployeeListItemDto, 'organizationUnitId'>): string {
    const byCatalog = this.organizationsCatalog().find((x) => x.id === row.organizationUnitId);
    if (byCatalog) {
      return this.i18n.lang() === 'ar' ? byCatalog.nameAr || byCatalog.nameEn : byCatalog.nameEn || byCatalog.nameAr;
    }
    return this.lookupName(this.organizations(), row.organizationUnitId) || row.organizationUnitId;
  }

  positionName(row: Pick<EmployeeListItemDto, 'positionId'>): string {
    const byCatalog = this.positionsCatalog().find((x) => x.id === row.positionId);
    if (byCatalog) {
      return this.i18n.lang() === 'ar' ? byCatalog.titleAr || byCatalog.titleEn : byCatalog.titleEn || byCatalog.titleAr;
    }
    return this.lookupName(this.positions(), row.positionId) || row.positionId;
  }

  openDetails(id: string): void {
    this.detailsBusy.set(true);
    this.detailsOpen.set(true);
    this.api.getById(id).subscribe({
      next: (employee) => {
        this.detailsItem.set(employee);
        this.detailsBusy.set(false);
      },
      error: () => {
        this.detailsBusy.set(false);
        this.detailsOpen.set(false);
        this.toast.show('تعذر تحميل بيانات الموظف', 'error');
      },
    });
  }

  closeDetails(): void {
    if (this.detailsBusy()) return;
    this.detailsOpen.set(false);
    this.detailsItem.set(null);
  }

  openEditFromDetails(): void {
    const item = this.detailsItem();
    if (!item) return;
    this.editId.set(item.id);
    this.editModel = {
      employeeNumber: item.employeeNumber,
      fullNameAr: item.fullNameAr,
      fullNameEn: item.fullNameEn,
      email: item.email,
      organizationUnitId: item.organizationUnitId,
      positionId: item.positionId,
    };
    this.detailsOpen.set(false);
    this.editOpen.set(true);
  }

  openEdit(id: string): void {
    this.editBusy.set(true);
    this.api.getById(id).subscribe({
      next: (employee) => {
        this.editId.set(employee.id);
        this.editModel = {
          employeeNumber: employee.employeeNumber,
          fullNameAr: employee.fullNameAr,
          fullNameEn: employee.fullNameEn,
          email: employee.email,
          organizationUnitId: employee.organizationUnitId,
          positionId: employee.positionId,
        };
        this.editOpen.set(true);
        this.editBusy.set(false);
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show('تعذر تحميل بيانات التعديل', 'error');
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
    const body: UpdateEmployeeRequest = {
      employeeNumber: this.editModel.employeeNumber.trim(),
      fullNameAr: this.editModel.fullNameAr.trim(),
      fullNameEn: this.editModel.fullNameEn.trim(),
      email: this.editModel.email.trim(),
      organizationUnitId: this.editModel.organizationUnitId.trim(),
      positionId: this.editModel.positionId.trim(),
    };

    if (!body.employeeNumber || !body.fullNameAr || !body.fullNameEn || !body.email || !body.organizationUnitId || !body.positionId) {
      this.toast.show(this.i18n.t('employees.toast.createRequired'), 'error');
      return;
    }

    this.editBusy.set(true);
    this.api.update(id, body).subscribe({
      next: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show('تم تحديث بيانات الموظف', 'success');
        this.load();
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show('تعذر تحديث بيانات الموظف', 'error');
      },
    });
  }

  private lookupName(items: readonly LookupItemDto[], id: string): string {
    return items.find((x) => x.id === id)?.name ?? '';
  }
}
