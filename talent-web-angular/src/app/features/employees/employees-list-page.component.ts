import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { EmployeesApiService } from '../../services/employees-api.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { CreateEmployeeRequest, EmployeeListItemDto } from '../../shared/models/employee.models';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-employees-list-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe],
  templateUrl: './employees-list-page.component.html',
  styleUrl: './employees-list-page.component.scss',
})
export class EmployeesListPageComponent implements OnInit {
  private readonly api = inject(EmployeesApiService);
  private readonly lookupApi = inject(IdentityLookupsApiService);
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
  readonly selectedOrgUnitName = computed(() => this.lookupName(this.organizations(), this.createModel.organizationUnitId));
  readonly selectedPositionName = computed(() => this.lookupName(this.positions(), this.createModel.positionId));

  createModel: CreateEmployeeRequest = {
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

  private lookupName(items: readonly LookupItemDto[], id: string): string {
    return items.find((x) => x.id === id)?.name ?? '';
  }
}
