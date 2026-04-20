import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { LayoutStateService } from '../layout-state.service';

interface NavItem {
  path: string;
  labelKey: string;
  permissions: readonly string[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);
  private readonly i18n = inject(I18nService);
  readonly layout = inject(LayoutStateService);
  readonly collapsed = signal(false);
  readonly tooltipOpen = signal(false);
  readonly tooltipText = signal('');
  readonly tooltipX = signal(0);
  readonly tooltipY = signal(0);
  readonly tooltipSide = signal<'left' | 'right'>('right');

  private readonly allItems: NavItem[] = [
    { path: '/dashboard', labelKey: 'nav.dashboard', permissions: [] },
    { path: '/employees', labelKey: 'nav.employees', permissions: [] },
    { path: '/users', labelKey: 'nav.users', permissions: [PermissionCodes.UserManage] },
    { path: '/roles', labelKey: 'nav.roles', permissions: [PermissionCodes.RoleManage] },
    { path: '/talent/performance', labelKey: 'nav.performance', permissions: [] },
    { path: '/talent/potential', labelKey: 'nav.potential', permissions: [] },
    { path: '/talent/nine-box', labelKey: 'nav.nineBox', permissions: [] },
    { path: '/succession', labelKey: 'nav.succession', permissions: [] },
    { path: '/development', labelKey: 'nav.development', permissions: [] },
    { path: '/marketplace', labelKey: 'nav.marketplace', permissions: [] },
    { path: '/analytics/executive', labelKey: 'nav.analytics', permissions: [] },
    {
      path: '/analytics/intelligence',
      labelKey: 'nav.intelligence',
      permissions: [PermissionCodes.IntelligenceView],
    },
    { path: '/analytics/insights', labelKey: 'nav.insights', permissions: [PermissionCodes.IntelligenceView] },
    {
      path: '/analytics/recommendations',
      labelKey: 'nav.recommendations',
      permissions: [PermissionCodes.IntelligenceView],
    },
    {
      path: '/approvals',
      labelKey: 'nav.approvals',
      permissions: [PermissionCodes.ApprovalRequestView],
    },
    { path: '/notifications', labelKey: 'nav.notifications', permissions: [] },
    {
      path: '/notification-templates',
      labelKey: 'nav.notificationTemplates',
      permissions: [PermissionCodes.NotificationView],
    },
    { path: '/settings', labelKey: 'nav.settings', permissions: [] },
  ];

  readonly visibleItems = computed(() =>
    this.allItems.filter((item) => item.permissions.length === 0 || this.auth.hasAnyPermission(item.permissions)),
  );

  readonly brandTitle = () => this.i18n.t('app.title');
  readonly brandSubtitle = () => this.i18n.t('app.subtitle');
  readonly collapseLabel = computed(() => this.i18n.t(this.collapsed() ? 'nav.expand' : 'nav.collapse'));

  toggleCollapsed(): void {
    this.collapsed.update((v) => !v);
    this.hideTooltip();
  }

  closeMobileSidebar(): void {
    this.layout.closeMobileSidebar();
  }

  showTooltip(event: MouseEvent, text: string): void {
    if (!this.collapsed()) return;
    const target = event.currentTarget as HTMLElement | null;
    if (!target) return;

    const rect = target.getBoundingClientRect();
    const desiredSide: 'left' | 'right' = this.i18n.isRtl() ? 'left' : 'right';
    const minSpaceForTooltip = 240;
    const canShowLeft = rect.left >= minSpaceForTooltip;
    const canShowRight = window.innerWidth - rect.right >= minSpaceForTooltip;
    const side =
      desiredSide === 'left'
        ? canShowLeft
          ? 'left'
          : 'right'
        : canShowRight
          ? 'right'
          : 'left';

    const centerY = rect.top + rect.height / 2;
    const clampedY = Math.min(Math.max(centerY, 28), window.innerHeight - 28);

    this.tooltipText.set(text);
    this.tooltipSide.set(side);
    this.tooltipX.set(side === 'left' ? rect.left - 10 : rect.right + 10);
    this.tooltipY.set(clampedY);
    this.tooltipOpen.set(true);
  }

  hideTooltip(): void {
    this.tooltipOpen.set(false);
  }

  tooltipLabel(labelKey: string): string {
    return this.i18n.t(labelKey);
  }

  iconFor(path: string): string {
    const icons: Record<string, string> = {
      '/dashboard': 'fa-solid fa-gauge-high',
      '/employees': 'fa-solid fa-users',
      '/users': 'fa-solid fa-user-shield',
      '/roles': 'fa-solid fa-user-gear',
      '/talent/performance': 'fa-solid fa-chart-line',
      '/talent/potential': 'fa-solid fa-seedling',
      '/talent/nine-box': 'fa-solid fa-table-cells-large',
      '/succession': 'fa-solid fa-sitemap',
      '/development': 'fa-solid fa-graduation-cap',
      '/marketplace': 'fa-solid fa-store',
      '/analytics/executive': 'fa-solid fa-chart-column',
      '/analytics/intelligence': 'fa-solid fa-brain',
      '/analytics/insights': 'fa-solid fa-lightbulb',
      '/analytics/recommendations': 'fa-solid fa-thumbs-up',
      '/approvals': 'fa-solid fa-file-signature',
      '/notifications': 'fa-solid fa-bell',
      '/notification-templates': 'fa-solid fa-envelope-open-text',
      '/settings': 'fa-solid fa-gear',
    };
    return icons[path] ?? 'fa-solid fa-circle-dot';
  }
}
