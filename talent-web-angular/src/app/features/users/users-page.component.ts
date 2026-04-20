import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { UsersApiService } from '../../services/users-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import {
  AssignUserRolesRequest,
  CreateUserRequest,
  UpdateUserRequest,
  UserDto,
  UserListItemDto,
} from '../../shared/models/user.models';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [FormsModule, DatePipe, TranslatePipe],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.scss',
})
export class UsersPageComponent implements OnInit {
  private readonly api = inject(UsersApiService);
  private readonly lookupApi = inject(IdentityLookupsApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  search = '';
  page = 1;
  readonly pageSize = 20;
  readonly viewMode = signal<ViewMode>('table');

  readonly result = signal<PagedResult<UserListItemDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);

  readonly roles = signal<LookupItemDto[]>([]);
  readonly employees = signal<LookupItemDto[]>([]);

  readonly createOpen = signal(false);
  readonly createBusy = signal(false);
  readonly createRoleIds = signal<string[]>([]);
  readonly createRoleDropdownOpen = signal(false);
  readonly createRoleSearch = signal('');
  createModel: CreateUserRequest = {
    userName: '',
    nameAr: '',
    nameEn: '',
    email: '',
    password: '',
    employeeId: null,
    roleIds: [],
  };

  readonly detailsOpen = signal(false);
  readonly detailsBusy = signal(false);
  readonly detailsUser = signal<UserDto | null>(null);

  readonly editOpen = signal(false);
  readonly editBusy = signal(false);
  readonly editUserId = signal<string | null>(null);
  editModel: UpdateUserRequest = {
    userName: '',
    nameAr: '',
    nameEn: '',
    email: '',
    employeeId: null,
    newPassword: null,
  };

  readonly rolesOpen = signal(false);
  readonly roleAssignBusy = signal(false);
  readonly roleUserId = signal<string | null>(null);
  readonly assignedRoleIds = signal<string[]>([]);
  readonly assignRoleDropdownOpen = signal(false);
  readonly assignRoleSearch = signal('');

  readonly filteredCreateRoles = computed(() => this.filterRoles(this.createRoleSearch()));
  readonly filteredAssignRoles = computed(() => this.filterRoles(this.assignRoleSearch()));

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
    this.lookupApi.getRoles('', 200).subscribe({
      next: (rows) => this.roles.set(rows),
      error: () => this.toast.show(this.i18n.t('users.toast.loadRolesFailed'), 'error'),
    });
    this.lookupApi.getEmployees('', 200).subscribe({
      next: (rows) => this.employees.set(rows),
      error: () => this.toast.show(this.i18n.t('users.toast.loadEmployeesFailed'), 'error'),
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
      userName: '',
      nameAr: '',
      nameEn: '',
      email: '',
      password: '',
      employeeId: null,
      roleIds: [],
    };
    this.createRoleIds.set([]);
    this.createRoleDropdownOpen.set(false);
    this.createRoleSearch.set('');
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (this.createBusy()) return;
    this.createRoleDropdownOpen.set(false);
    this.createOpen.set(false);
  }

  toggleCreateRoleDropdown(): void {
    this.createRoleDropdownOpen.update((v) => !v);
  }

  toggleCreateRole(id: string): void {
    const next = new Set(this.createRoleIds());
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    this.createRoleIds.set(Array.from(next));
  }

  createRoleSelected(id: string): boolean {
    return this.createRoleIds().includes(id);
  }

  createRoleSummary(): string {
    const ids = this.createRoleIds();
    if (ids.length === 0) return this.i18n.t('users.roles.select');
    const roleMap = new Map(this.roles().map((r) => [r.id, r.name]));
    return ids.map((id) => roleMap.get(id) ?? id).join('، ');
  }

  saveCreate(): void {
    if (!this.createModel.userName.trim() || !this.createModel.email.trim() || !this.createModel.password.trim()) {
      this.toast.show(this.i18n.t('users.toast.createRequired'), 'error');
      return;
    }
    if (this.createRoleIds().length === 0) {
      this.toast.show(this.i18n.t('users.toast.rolesRequired'), 'error');
      return;
    }

    this.createBusy.set(true);
    const body: CreateUserRequest = {
      ...this.createModel,
      userName: this.createModel.userName.trim(),
      nameAr: this.createModel.nameAr?.trim() ? this.createModel.nameAr.trim() : null,
      nameEn: this.createModel.nameEn?.trim() ? this.createModel.nameEn.trim() : null,
      email: this.createModel.email.trim(),
      password: this.createModel.password,
      employeeId: this.normalizeNullableGuid(this.createModel.employeeId),
      roleIds: this.createRoleIds(),
    };

    this.api.create(body).subscribe({
      next: () => {
        this.createBusy.set(false);
        this.createOpen.set(false);
        this.toast.show(this.i18n.t('users.toast.created'), 'success');
        this.page = 1;
        this.load();
      },
      error: () => {
        this.createBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.createFailed'), 'error');
      },
    });
  }

  openDetails(id: string): void {
    this.detailsBusy.set(true);
    this.detailsOpen.set(true);
    this.api.getById(id).subscribe({
      next: (u) => {
        this.detailsUser.set(u);
        this.detailsBusy.set(false);
      },
      error: () => {
        this.detailsBusy.set(false);
        this.detailsOpen.set(false);
        this.toast.show(this.i18n.t('users.toast.loadDetailsFailed'), 'error');
      },
    });
  }

  closeDetails(): void {
    if (this.detailsBusy()) return;
    this.detailsOpen.set(false);
    this.detailsUser.set(null);
  }

  openEdit(id: string): void {
    this.detailsOpen.set(false);
    this.editOpen.set(true);
    this.editBusy.set(true);
    this.editUserId.set(id);
    this.api.getById(id).subscribe({
      next: (u) => {
        this.editModel = {
          userName: u.userName,
          nameAr: u.nameAr ?? '',
          nameEn: u.nameEn ?? '',
          email: u.email,
          employeeId: u.employeeId ?? null,
          newPassword: null,
        };
        this.editBusy.set(false);
      },
      error: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('users.toast.loadEditFailed'), 'error');
      },
    });
  }

  closeEdit(): void {
    if (this.editBusy()) return;
    this.editOpen.set(false);
    this.editUserId.set(null);
  }

  saveEdit(): void {
    const id = this.editUserId();
    if (!id) return;
    if (!this.editModel.userName.trim() || !this.editModel.email.trim()) {
      this.toast.show(this.i18n.t('users.toast.editRequired'), 'error');
      return;
    }

    this.editBusy.set(true);
    const body: UpdateUserRequest = {
      userName: this.editModel.userName.trim(),
      nameAr: this.editModel.nameAr?.trim() ? this.editModel.nameAr.trim() : null,
      nameEn: this.editModel.nameEn?.trim() ? this.editModel.nameEn.trim() : null,
      email: this.editModel.email.trim(),
      employeeId: this.normalizeNullableGuid(this.editModel.employeeId),
      newPassword: this.editModel.newPassword?.trim() ? this.editModel.newPassword.trim() : null,
    };

    this.api.update(id, body).subscribe({
      next: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('users.toast.updated'), 'success');
        this.load();
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.updateFailed'), 'error');
      },
    });
  }

  openRoles(id: string): void {
    this.detailsOpen.set(false);
    this.rolesOpen.set(true);
    this.roleAssignBusy.set(true);
    this.roleUserId.set(id);
    this.assignRoleSearch.set('');
    this.assignRoleDropdownOpen.set(false);
    this.api.getById(id).subscribe({
      next: (u) => {
        this.assignedRoleIds.set(this.roleIdsFromNames(u.roleNames));
        this.roleAssignBusy.set(false);
      },
      error: () => {
        this.roleAssignBusy.set(false);
        this.rolesOpen.set(false);
        this.toast.show(this.i18n.t('users.toast.loadUserRolesFailed'), 'error');
      },
    });
  }

  closeRoles(): void {
    if (this.roleAssignBusy()) return;
    this.rolesOpen.set(false);
    this.roleUserId.set(null);
    this.assignRoleDropdownOpen.set(false);
  }

  toggleAssignRoleDropdown(): void {
    this.assignRoleDropdownOpen.update((v) => !v);
  }

  toggleAssignedRole(id: string): void {
    const next = new Set(this.assignedRoleIds());
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    this.assignedRoleIds.set(Array.from(next));
  }

  assignedRoleSelected(id: string): boolean {
    return this.assignedRoleIds().includes(id);
  }

  assignRoleSummary(): string {
    const ids = this.assignedRoleIds();
    if (ids.length === 0) return this.i18n.t('users.roles.select');
    const roleMap = new Map(this.roles().map((r) => [r.id, r.name]));
    return ids.map((id) => roleMap.get(id) ?? id).join('، ');
  }

  saveRoles(): void {
    const id = this.roleUserId();
    if (!id) return;
    if (this.assignedRoleIds().length === 0) {
      this.toast.show(this.i18n.t('users.toast.rolesRequired'), 'error');
      return;
    }

    this.roleAssignBusy.set(true);
    const body: AssignUserRolesRequest = { roleIds: this.assignedRoleIds() };
    this.api.assignRoles(id, body).subscribe({
      next: (u) => {
        this.roleAssignBusy.set(false);
        this.rolesOpen.set(false);
        this.assignedRoleIds.set(this.roleIdsFromNames(u.roleNames));
        this.toast.show(this.i18n.t('users.toast.rolesUpdated'), 'success');
        this.load();
      },
      error: () => {
        this.roleAssignBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.rolesUpdateFailed'), 'error');
      },
    });
  }

  activateSelected(): void {
    const id = this.detailsUser()?.id;
    if (!id) return;
    this.detailsBusy.set(true);
    this.api.activate(id).subscribe({
      next: () => {
        this.detailsBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.activated'), 'success');
        this.refreshDetails(id);
        this.load();
      },
      error: () => {
        this.detailsBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.activateFailed'), 'error');
      },
    });
  }

  deactivateSelected(): void {
    const id = this.detailsUser()?.id;
    if (!id) return;
    this.detailsBusy.set(true);
    this.api.deactivate(id).subscribe({
      next: () => {
        this.detailsBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.deactivated'), 'success');
        this.refreshDetails(id);
        this.load();
      },
      error: () => {
        this.detailsBusy.set(false);
        this.toast.show(this.i18n.t('users.toast.deactivateFailed'), 'error');
      },
    });
  }

  private refreshDetails(id: string): void {
    this.api.getById(id).subscribe({
      next: (u) => this.detailsUser.set(u),
      error: () => this.toast.show(this.i18n.t('users.toast.refreshDetailsFailed'), 'error'),
    });
  }

  private normalizeNullableGuid(value?: string | null): string | null {
    if (!value) return null;
    const v = value.trim();
    return v ? v : null;
  }

  private filterRoles(search: string): LookupItemDto[] {
    const term = search.trim().toLowerCase();
    if (!term) return this.roles();
    return this.roles().filter((r) => r.name.toLowerCase().includes(term));
  }

  userDisplayName(user: Pick<UserListItemDto, 'userName' | 'nameAr' | 'nameEn'>): string {
    if (this.i18n.lang() === 'ar') {
      return user.nameAr?.trim() || user.nameEn?.trim() || user.userName;
    }
    return user.nameEn?.trim() || user.nameAr?.trim() || user.userName;
  }

  private roleIdsFromNames(roleNames: readonly string[]): string[] {
    if (roleNames.length === 0) return [];
    const map = new Map(this.roles().map((r) => [r.name.toLowerCase(), r.id]));
    return roleNames
      .map((n) => map.get(n.toLowerCase()) ?? null)
      .filter((v): v is string => v !== null);
  }
}
