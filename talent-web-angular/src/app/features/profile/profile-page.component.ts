import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { UsersApiService } from '../../services/users-api.service';
import { UpdateUserRequest, UserDto } from '../../shared/models/user.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [TranslatePipe, DatePipe, FormsModule],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss',
})
export class ProfilePageComponent {
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  private readonly usersApi = inject(UsersApiService);
  private readonly toast = inject(ToastService);

  readonly editOpen = signal(false);
  readonly editBusy = signal(false);
  readonly avatarUrl = signal<string | null>(null);
  readonly avatarActionsOpen = signal(false);
  readonly avatarSeed = computed(() => this.auth.sessionSnapshot()?.email ?? this.auth.sessionSnapshot()?.userName ?? 'user');
  private readonly avatarStoragePrefix = 'talent_os_profile_avatar_';

  editModel: UpdateUserRequest = {
    userName: '',
    nameAr: '',
    nameEn: '',
    email: '',
    employeeId: null,
    newPassword: null,
  };

  constructor() {
    this.restoreAvatar();
  }

  readonly permissionPages = computed(() => {
    const permissions = this.auth.sessionSnapshot()?.permissions ?? [];
    return permissions.map((code) => this.permissionDisplayLabel(code));
  });

  openEdit(): void {
    const session = this.auth.sessionSnapshot();
    if (!session?.userId) {
      this.toast.show(this.i18n.t('profile.toast.loadEditFailed'), 'error');
      return;
    }

    this.editBusy.set(true);
    this.editOpen.set(true);
    this.usersApi.getById(session.userId).subscribe({
      next: (user) => {
        this.setEditModel(user);
        this.editBusy.set(false);
      },
      error: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('profile.toast.loadEditFailed'), 'error');
      },
    });
  }

  closeEdit(): void {
    if (this.editBusy()) return;
    this.editOpen.set(false);
  }

  saveEdit(): void {
    const session = this.auth.sessionSnapshot();
    if (!session?.userId) return;

    if (!this.editModel.userName.trim() || !this.editModel.email.trim()) {
      this.toast.show(this.i18n.t('profile.toast.editRequired'), 'error');
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

    this.usersApi.update(session.userId, body).subscribe({
      next: () => {
        this.editBusy.set(false);
        this.editOpen.set(false);
        this.toast.show(this.i18n.t('profile.toast.updated'), 'success');
      },
      error: () => {
        this.editBusy.set(false);
        this.toast.show(this.i18n.t('profile.toast.updateFailed'), 'error');
      },
    });
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toast.show(this.i18n.t('profile.toast.avatarInvalidType'), 'error');
      return;
    }

    const maxBytes = 2 * 1024 * 1024;
    if (file.size > maxBytes) {
      this.toast.show(this.i18n.t('profile.toast.avatarTooLarge'), 'error');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const value = typeof reader.result === 'string' ? reader.result : null;
      if (!value) return;
      this.avatarUrl.set(value);
      this.persistAvatar(value);
      this.avatarActionsOpen.set(false);
      this.toast.show(this.i18n.t('profile.toast.avatarSaved'), 'success');
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  removeAvatar(): void {
    this.avatarUrl.set(null);
    this.clearAvatar();
    this.avatarActionsOpen.set(false);
    this.toast.show(this.i18n.t('profile.toast.avatarRemoved'), 'success');
  }

  toggleAvatarActions(): void {
    this.avatarActionsOpen.update((v) => !v);
  }

  closeAvatarActions(): void {
    this.avatarActionsOpen.set(false);
  }

  permissionIcon(permissionLabel: string): string {
    if (permissionLabel.includes(this.i18n.t('profile.permissions.action.manage'))) return 'fa-solid fa-shield-halved';
    if (permissionLabel.includes(this.i18n.t('profile.permissions.action.edit'))) return 'fa-solid fa-pen-to-square';
    if (permissionLabel.includes(this.i18n.t('profile.permissions.action.view'))) return 'fa-regular fa-eye';
    if (permissionLabel.includes(this.i18n.t('profile.permissions.action.create'))) return 'fa-solid fa-plus';
    if (permissionLabel.includes(this.i18n.t('profile.permissions.action.approve'))) return 'fa-solid fa-circle-check';
    return 'fa-solid fa-key';
  }

  private permissionDisplayLabel(code: string): string {
    const page = this.pageLabelFromPermission(code);
    const action = this.actionLabelFromPermission(code);
    return `${page} - ${action}`;
  }

  private pageLabelFromPermission(code: string): string {
    const normalized = code.trim().toUpperCase();
    if (!normalized) return this.i18n.t('profile.permissions.other');

    if (normalized.startsWith('USER_')) return this.i18n.t('nav.users');
    if (normalized.startsWith('ROLE_')) return this.i18n.t('nav.roles');
    if (normalized.startsWith('EMPLOYEE_')) return this.i18n.t('nav.employees');
    if (normalized.startsWith('PERFORMANCE_')) return this.i18n.t('nav.performance');
    if (normalized.startsWith('POTENTIAL_')) return this.i18n.t('nav.potential');
    if (normalized.startsWith('SCORING_') || normalized.startsWith('CLASSIFICATION_')) {
      return this.i18n.t('nav.nineBox');
    }
    if (normalized.startsWith('SUCCESSION_')) return this.i18n.t('nav.succession');
    if (normalized.startsWith('DEVELOPMENT_')) return this.i18n.t('nav.development');
    if (normalized.startsWith('MARKETPLACE_')) return this.i18n.t('nav.marketplace');
    if (normalized.startsWith('ANALYTICS_')) return this.i18n.t('nav.analytics');
    if (normalized.startsWith('INTELLIGENCE_')) return this.i18n.t('nav.intelligence');
    if (normalized.startsWith('APPROVAL_REQUEST_')) return this.i18n.t('nav.approvals');
    if (normalized.startsWith('NOTIFICATION_')) return this.i18n.t('nav.notifications');
    if (normalized.startsWith('COMPETENCY_')) return this.i18n.t('profile.permissions.competency');

    return this.i18n.t('profile.permissions.other');
  }

  private actionLabelFromPermission(code: string): string {
    const normalized = code.trim().toUpperCase();
    const action = normalized.split('_').at(-1) ?? '';
    switch (action) {
      case 'VIEW':
        return this.i18n.t('profile.permissions.action.view');
      case 'MANAGE':
        return this.i18n.t('profile.permissions.action.manage');
      case 'EDIT':
        return this.i18n.t('profile.permissions.action.edit');
      case 'CREATE':
        return this.i18n.t('profile.permissions.action.create');
      case 'ASSIGN':
        return this.i18n.t('profile.permissions.action.assign');
      case 'REVIEW':
        return this.i18n.t('profile.permissions.action.review');
      case 'APPROVE':
        return this.i18n.t('profile.permissions.action.approve');
      case 'REJECT':
        return this.i18n.t('profile.permissions.action.reject');
      case 'APPLY':
        return this.i18n.t('profile.permissions.action.apply');
      case 'GENERATE':
        return this.i18n.t('profile.permissions.action.generate');
      default:
        return this.i18n.t('profile.permissions.action.access');
    }
  }

  private setEditModel(user: UserDto): void {
    this.editModel = {
      userName: user.userName,
      nameAr: user.nameAr ?? '',
      nameEn: user.nameEn ?? '',
      email: user.email,
      employeeId: user.employeeId ?? null,
      newPassword: null,
    };
  }

  private normalizeNullableGuid(value?: string | null): string | null {
    if (!value) return null;
    const v = value.trim();
    return v ? v : null;
  }

  private avatarStorageKey(): string | null {
    const id = this.auth.sessionSnapshot()?.userId;
    return id ? `${this.avatarStoragePrefix}${id}` : null;
  }

  private restoreAvatar(): void {
    const key = this.avatarStorageKey();
    if (!key) return;
    const stored = localStorage.getItem(key);
    if (stored) this.avatarUrl.set(stored);
  }

  private persistAvatar(value: string): void {
    const key = this.avatarStorageKey();
    if (!key) return;
    localStorage.setItem(key, value);
  }

  private clearAvatar(): void {
    const key = this.avatarStorageKey();
    if (!key) return;
    localStorage.removeItem(key);
  }
}
