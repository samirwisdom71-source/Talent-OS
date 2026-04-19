import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

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

  private readonly allItems: NavItem[] = [
    { path: '/dashboard', labelKey: 'nav.dashboard', permissions: [] },
    { path: '/employees', labelKey: 'nav.employees', permissions: [] },
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

  iconFor(path: string): string {
    const icons: Record<string, string> = {
      '/dashboard': '📊',
      '/employees': '👥',
      '/talent/performance': '📆',
      '/talent/potential': '💡',
      '/talent/nine-box': '9️⃣',
      '/succession': '🔀',
      '/development': '🎯',
      '/marketplace': '🏪',
      '/analytics/executive': '📈',
      '/analytics/intelligence': '🧠',
      '/analytics/insights': '💬',
      '/analytics/recommendations': '✅',
      '/approvals': '✍️',
      '/notifications': '🔔',
      '/notification-templates': '📝',
      '/settings': '⚙️',
    };
    return icons[path] ?? '·';
  }
}
