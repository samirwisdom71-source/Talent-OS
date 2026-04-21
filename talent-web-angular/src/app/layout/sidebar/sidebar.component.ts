import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
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

interface NavGroup {
  id: string;
  titleKey: string;
  items: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);
  private readonly i18n = inject(I18nService);
  private readonly router = inject(Router);
  readonly layout = inject(LayoutStateService);
  readonly collapsed = signal(false);
  readonly tooltipOpen = signal(false);
  readonly tooltipText = signal('');
  readonly tooltipX = signal(0);
  readonly tooltipY = signal(0);
  readonly tooltipSide = signal<'left' | 'right'>('right');

  /** Multi-item groups: expanded when id is in this set. Default: all collapsed until the user opens a section. */
  private readonly expandedGroupIds = signal<ReadonlySet<string>>(new Set<string>());

  private readonly allGroups: NavGroup[] = [
    {
      id: 'overview',
      titleKey: 'nav.group.overview',
      items: [{ path: '/dashboard', labelKey: 'nav.dashboard', permissions: [] }],
    },
    {
      id: 'people',
      titleKey: 'nav.group.people',
      items: [
        { path: '/employees', labelKey: 'nav.employees', permissions: [] },
        { path: '/organization-units', labelKey: 'nav.organizationUnits', permissions: [PermissionCodes.EmployeeEdit] },
        { path: '/positions', labelKey: 'nav.positions', permissions: [PermissionCodes.EmployeeEdit] },
        {
          path: '/succession/critical-positions',
          labelKey: 'nav.criticalPositions',
          permissions: [PermissionCodes.SuccessionView, PermissionCodes.SuccessionManage],
        },
        { path: '/job-grades', labelKey: 'nav.jobGrades', permissions: [PermissionCodes.EmployeeEdit] },
      ],
    },
    {
      id: 'identity',
      titleKey: 'nav.group.identity',
      items: [
        { path: '/users', labelKey: 'nav.users', permissions: [PermissionCodes.UserManage] },
        { path: '/roles', labelKey: 'nav.roles', permissions: [PermissionCodes.RoleManage] },
      ],
    },
    {
      id: 'competencies',
      titleKey: 'nav.group.competencies',
      items: [
        {
          path: '/competency-categories',
          labelKey: 'nav.competencyCategories',
          permissions: [PermissionCodes.CompetencyEdit],
        },
        { path: '/competencies', labelKey: 'nav.competencies', permissions: [PermissionCodes.CompetencyEdit] },
        { path: '/competency-levels', labelKey: 'nav.competencyLevels', permissions: [PermissionCodes.CompetencyEdit] },
        {
          path: '/job-competency-requirements',
          labelKey: 'nav.jobCompetencyRequirements',
          permissions: [PermissionCodes.CompetencyEdit],
        },
      ],
    },
    {
      id: 'talent',
      titleKey: 'nav.group.talent',
      items: [
        { path: '/talent/performance', labelKey: 'nav.performance', permissions: [] },
        { path: '/talent/potential', labelKey: 'nav.potential', permissions: [] },
        { path: '/talent/nine-box', labelKey: 'nav.nineBox', permissions: [] },
        { path: '/scoring-policies', labelKey: 'nav.scoringPolicies', permissions: [PermissionCodes.ScoringView, PermissionCodes.ScoringManage] },
        { path: '/talent/analytics', labelKey: 'nav.talentAnalytics', permissions: [] },
        { path: '/succession', labelKey: 'nav.succession', permissions: [] },
        { path: '/development', labelKey: 'nav.development', permissions: [] },
        { path: '/marketplace', labelKey: 'nav.marketplace', permissions: [] },
      ],
    },
    {
      id: 'analytics',
      titleKey: 'nav.group.analytics',
      items: [
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
      ],
    },
    {
      id: 'workflow',
      titleKey: 'nav.group.workflow',
      items: [
        {
          path: '/approvals',
          labelKey: 'nav.approvals',
          permissions: [PermissionCodes.ApprovalRequestView],
        },
      ],
    },
    {
      id: 'notifications',
      titleKey: 'nav.group.notifications',
      items: [
        { path: '/notifications', labelKey: 'nav.notifications', permissions: [] },
        {
          path: '/notification-templates',
          labelKey: 'nav.notificationTemplates',
          permissions: [PermissionCodes.NotificationView],
        },
      ],
    },
    {
      id: 'system',
      titleKey: 'nav.group.system',
      items: [{ path: '/settings', labelKey: 'nav.settings', permissions: [] }],
    },
  ];

  readonly visibleGroups = computed(() =>
    this.allGroups
      .map((g) => ({
        ...g,
        items: g.items.filter(
          (item) => item.permissions.length === 0 || this.auth.hasAnyPermission(item.permissions),
        ),
      }))
      .filter((g) => g.items.length > 0),
  );

  readonly visibleFlatItems = computed(() => this.visibleGroups().flatMap((g) => g.items));

  readonly brandTitle = () => this.i18n.t('app.title');
  readonly brandSubtitle = () => this.i18n.t('app.subtitle');
  readonly collapseLabel = computed(() => this.i18n.t(this.collapsed() ? 'nav.expand' : 'nav.collapse'));

  isGroupExpanded(groupId: string): boolean {
    return this.expandedGroupIds().has(groupId);
  }

  toggleGroup(groupId: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.expandedGroupIds.update((current) => {
      const next = new Set(current);
      if (next.has(groupId)) next.delete(groupId);
      else next.add(groupId);
      return next;
    });
  }

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
      '/organization-units': 'fa-solid fa-building',
      '/positions': 'fa-solid fa-briefcase',
      '/job-grades': 'fa-solid fa-layer-group',
      '/users': 'fa-solid fa-user-shield',
      '/roles': 'fa-solid fa-user-gear',
      '/competency-categories': 'fa-solid fa-layer-group',
      '/competencies': 'fa-solid fa-list-check',
      '/competency-levels': 'fa-solid fa-stairs',
      '/job-competency-requirements': 'fa-solid fa-link',
      '/talent/performance': 'fa-solid fa-chart-line',
      '/talent/potential': 'fa-solid fa-seedling',
      '/talent/nine-box': 'fa-solid fa-table-cells-large',
      '/scoring-policies': 'fa-solid fa-scale-balanced',
      '/talent/analytics': 'fa-solid fa-chart-pie',
      '/talent/classifications': 'fa-solid fa-tags',
      '/talent/scores': 'fa-solid fa-calculator',
      '/succession': 'fa-solid fa-sitemap',
      '/succession/plans': 'fa-solid fa-list-check',
      '/succession/analytics': 'fa-solid fa-chart-simple',
      '/succession/successor-candidates': 'fa-solid fa-user-group',
      '/succession/critical-positions': 'fa-solid fa-bullseye',
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

  /**
   * «التعاقب» يبقى مُضاءً لـ /succession و /succession/plans/… فقط،
   * ولا يُفعَّل عند /succession/critical-positions.
   */
  linkIsActive(path: string): boolean {
    const current = this.normalizePath(this.router.url);
    if (path === '/dashboard') {
      return current === '/dashboard' || current === '';
    }
    if (path === '/talent/performance') {
      return current === '/talent/performance' || current.startsWith('/talent/performance/');
    }
    if (path === '/talent/analytics') {
      return (
        current === '/talent/analytics' ||
        current === '/talent/classifications' ||
        current === '/talent/scores'
      );
    }
    if (path === '/succession') {
      if (current === '/succession/critical-positions') return false;
      return (
        current === '/succession' ||
        current.startsWith('/succession/plans') ||
        current === '/succession/analytics' ||
        current === '/succession/successor-candidates'
      );
    }
    if (path === '/succession/critical-positions') {
      return current === '/succession/critical-positions';
    }
    return this.router.isActive(path, {
      paths: 'subset',
      queryParams: 'ignored',
      fragment: 'ignored',
      matrixParams: 'ignored',
    });
  }

  private normalizePath(url: string): string {
    const p = (url.split('?')[0] ?? url).trim();
    if (p.length > 1 && p.endsWith('/')) {
      return p.slice(0, -1);
    }
    return p;
  }
}
