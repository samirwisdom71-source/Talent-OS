import { DatePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [TranslatePipe, DatePipe],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss',
})
export class ProfilePageComponent {
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly permissionPages = computed(() => {
    const permissions = this.auth.sessionSnapshot()?.permissions ?? [];
    return permissions.map((code) => this.permissionDisplayLabel(code));
  });

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
}
