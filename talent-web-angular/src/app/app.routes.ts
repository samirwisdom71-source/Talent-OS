import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell/app-shell.component').then((m) => m.AppShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page.component').then((m) => m.DashboardPageComponent),
      },
      {
        path: 'employees',
        loadComponent: () =>
          import('./features/employees/employees-list-page.component').then((m) => m.EmployeesListPageComponent),
      },
      {
        path: 'organization-units',
        canActivate: [permissionGuard],
        data: { permissions: ['EMPLOYEE_EDIT'] },
        loadComponent: () =>
          import('./features/organization-units/organization-units-page.component').then(
            (m) => m.OrganizationUnitsPageComponent,
          ),
      },
      {
        path: 'positions',
        canActivate: [permissionGuard],
        data: { permissions: ['EMPLOYEE_EDIT'] },
        loadComponent: () => import('./features/positions/positions-page.component').then((m) => m.PositionsPageComponent),
      },
      {
        path: 'job-grades',
        canActivate: [permissionGuard],
        data: { permissions: ['EMPLOYEE_EDIT'] },
        loadComponent: () => import('./features/job-grades/job-grades-page.component').then((m) => m.JobGradesPageComponent),
      },
      {
        path: 'employees/create',
        canActivate: [permissionGuard],
        data: { permissions: ['EMPLOYEE_EDIT'] },
        loadComponent: () =>
          import('./features/employees/employee-create-page.component').then((m) => m.EmployeeCreatePageComponent),
      },
      {
        path: 'employees/:id',
        loadComponent: () =>
          import('./features/employees/employee-detail-page.component').then((m) => m.EmployeeDetailPageComponent),
      },
      {
        path: 'users',
        canActivate: [permissionGuard],
        data: { permissions: ['USER_MANAGE'] },
        loadComponent: () => import('./features/users/users-page.component').then((m) => m.UsersPageComponent),
      },
      {
        path: 'roles',
        canActivate: [permissionGuard],
        data: { permissions: ['ROLE_MANAGE'] },
        loadComponent: () => import('./features/roles/roles-page.component').then((m) => m.RolesPageComponent),
      },
      {
        path: 'competency-categories',
        canActivate: [permissionGuard],
        data: { permissions: ['COMPETENCY_EDIT'] },
        loadComponent: () =>
          import('./features/competency-categories/competency-categories-page.component').then(
            (m) => m.CompetencyCategoriesPageComponent,
          ),
      },
      {
        path: 'competencies',
        canActivate: [permissionGuard],
        data: { permissions: ['COMPETENCY_EDIT'] },
        loadComponent: () =>
          import('./features/competencies/competencies-page.component').then((m) => m.CompetenciesPageComponent),
      },
      {
        path: 'competency-levels',
        canActivate: [permissionGuard],
        data: { permissions: ['COMPETENCY_EDIT'] },
        loadComponent: () =>
          import('./features/competency-levels/competency-levels-page.component').then(
            (m) => m.CompetencyLevelsPageComponent,
          ),
      },
      {
        path: 'job-competency-requirements',
        canActivate: [permissionGuard],
        data: { permissions: ['COMPETENCY_EDIT'] },
        loadComponent: () =>
          import('./features/job-competency-requirements/job-competency-requirements-page.component').then(
            (m) => m.JobCompetencyRequirementsPageComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile-page.component').then((m) => m.ProfilePageComponent),
      },
      {
        path: 'talent/performance',
        loadComponent: () =>
          import('./features/talent/performance-page.component').then((m) => m.PerformancePageComponent),
      },
      {
        path: 'talent/performance/cycles',
        loadComponent: () =>
          import('./features/talent/performance-cycles-page.component').then((m) => m.PerformanceCyclesPageComponent),
      },
      {
        path: 'talent/performance/evaluations',
        loadComponent: () =>
          import('./features/talent/performance-evaluations-page.component').then(
            (m) => m.PerformanceEvaluationsPageComponent,
          ),
      },
      {
        path: 'talent/performance/goals',
        loadComponent: () =>
          import('./features/talent/performance-goals-page.component').then((m) => m.PerformanceGoalsPageComponent),
      },
      {
        path: 'talent/potential',
        loadComponent: () =>
          import('./features/talent/potential-page.component').then((m) => m.PotentialPageComponent),
      },
      {
        path: 'talent/nine-box',
        loadComponent: () =>
          import('./features/talent/nine-box-page.component').then((m) => m.NineBoxPageComponent),
      },
      {
        path: 'talent/analytics',
        loadComponent: () =>
          import('./features/talent/talent-analytics-page.component').then((m) => m.TalentAnalyticsPageComponent),
      },
      {
        path: 'talent/classifications',
        canActivate: [permissionGuard],
        data: { permissions: ['CLASSIFICATION_VIEW', 'CLASSIFICATION_MANAGE'] },
        loadComponent: () =>
          import('./features/talent/talent-classifications-admin-page.component').then(
            (m) => m.TalentClassificationsAdminPageComponent,
          ),
      },
      {
        path: 'talent/scores',
        canActivate: [permissionGuard],
        data: { permissions: ['CLASSIFICATION_VIEW', 'CLASSIFICATION_MANAGE'] },
        loadComponent: () =>
          import('./features/talent/talent-scores-page.component').then((m) => m.TalentScoresPageComponent),
      },
      {
        path: 'scoring-policies',
        canActivate: [permissionGuard],
        data: { permissions: ['SCORING_VIEW', 'SCORING_MANAGE'] },
        loadComponent: () =>
          import('./features/scoring-policies/scoring-policies-page.component').then((m) => m.ScoringPoliciesPageComponent),
      },
      {
        path: 'succession',
        loadComponent: () =>
          import('./features/succession/succession-overview-page.component').then(
            (m) => m.SuccessionOverviewPageComponent,
          ),
      },
      {
        path: 'succession/critical-positions',
        canActivate: [permissionGuard],
        data: { permissions: ['SUCCESSION_VIEW', 'SUCCESSION_MANAGE'] },
        loadComponent: () =>
          import('./features/critical-positions/critical-positions-page.component').then(
            (m) => m.CriticalPositionsPageComponent,
          ),
      },
      {
        path: 'succession/plans',
        canActivate: [permissionGuard],
        data: { permissions: ['SUCCESSION_VIEW', 'SUCCESSION_MANAGE'] },
        loadComponent: () =>
          import('./features/succession/succession-plans-list-page.component').then(
            (m) => m.SuccessionPlansListPageComponent,
          ),
      },
      {
        path: 'succession/analytics',
        canActivate: [permissionGuard],
        data: { permissions: ['SUCCESSION_VIEW', 'SUCCESSION_MANAGE'] },
        loadComponent: () =>
          import('./features/succession/succession-analytics-page.component').then(
            (m) => m.SuccessionAnalyticsPageComponent,
          ),
      },
      {
        path: 'succession/successor-candidates',
        canActivate: [permissionGuard],
        data: { permissions: ['SUCCESSION_VIEW', 'SUCCESSION_MANAGE'] },
        loadComponent: () =>
          import('./features/succession/successor-candidates-page.component').then(
            (m) => m.SuccessorCandidatesPageComponent,
          ),
      },
      {
        path: 'succession/plans/create',
        canActivate: [permissionGuard],
        data: { permissions: ['SUCCESSION_MANAGE'] },
        loadComponent: () =>
          import('./features/succession/succession-plan-create-page.component').then(
            (m) => m.SuccessionPlanCreatePageComponent,
          ),
      },
      {
        path: 'succession/plans/:id',
        loadComponent: () =>
          import('./features/succession/succession-plan-detail-page.component').then(
            (m) => m.SuccessionPlanDetailPageComponent,
          ),
      },
      {
        path: 'development',
        loadComponent: () =>
          import('./features/development/development-list-page.component').then((m) => m.DevelopmentListPageComponent),
      },
      {
        path: 'development/create',
        canActivate: [permissionGuard],
        data: { permissions: ['DEVELOPMENT_MANAGE'] },
        loadComponent: () =>
          import('./features/development/development-create-page.component').then(
            (m) => m.DevelopmentCreatePageComponent,
          ),
      },
      {
        path: 'development/:id',
        loadComponent: () =>
          import('./features/development/development-detail-page.component').then(
            (m) => m.DevelopmentDetailPageComponent,
          ),
      },
      {
        path: 'marketplace',
        loadComponent: () =>
          import('./features/marketplace/marketplace-list-page.component').then((m) => m.MarketplaceListPageComponent),
      },
      {
        path: 'marketplace/opportunities',
        canActivate: [permissionGuard],
        data: { permissions: ['MARKETPLACE_APPLY'] },
        loadComponent: () =>
          import('./features/marketplace/marketplace-opportunities-employee-page.component').then(
            (m) => m.MarketplaceOpportunitiesEmployeePageComponent,
          ),
      },
      {
        path: 'marketplace/opportunities/:id',
        canActivate: [permissionGuard],
        data: { permissions: ['MARKETPLACE_APPLY'] },
        loadComponent: () =>
          import('./features/marketplace/marketplace-opportunity-employee-detail-page.component').then(
            (m) => m.MarketplaceOpportunityEmployeeDetailPageComponent,
          ),
      },
      {
        path: 'marketplace/opportunities/:id/apply',
        canActivate: [permissionGuard],
        data: { permissions: ['MARKETPLACE_APPLY'] },
        loadComponent: () =>
          import('./features/marketplace/marketplace-opportunity-apply-page.component').then(
            (m) => m.MarketplaceOpportunityApplyPageComponent,
          ),
      },
      {
        path: 'marketplace/create',
        canActivate: [permissionGuard],
        data: { permissions: ['MARKETPLACE_MANAGE'] },
        loadComponent: () =>
          import('./features/marketplace/marketplace-create-page.component').then(
            (m) => m.MarketplaceCreatePageComponent,
          ),
      },
      {
        path: 'marketplace/:id',
        loadComponent: () =>
          import('./features/marketplace/marketplace-detail-page.component').then(
            (m) => m.MarketplaceDetailPageComponent,
          ),
      },
      {
        path: 'analytics/executive',
        loadComponent: () =>
          import('./features/analytics/executive-analytics-page.component').then(
            (m) => m.ExecutiveAnalyticsPageComponent,
          ),
      },
      {
        path: 'analytics/intelligence',
        canActivate: [permissionGuard],
        data: { permissions: ['INTELLIGENCE_VIEW'] },
        loadComponent: () =>
          import('./features/analytics/intelligence-overview-page.component').then(
            (m) => m.IntelligenceOverviewPageComponent,
          ),
      },
      {
        path: 'analytics/insights',
        canActivate: [permissionGuard],
        data: { permissions: ['INTELLIGENCE_VIEW'] },
        loadComponent: () =>
          import('./features/analytics/insights-list-page.component').then((m) => m.InsightsListPageComponent),
      },
      {
        path: 'analytics/recommendations',
        canActivate: [permissionGuard],
        data: { permissions: ['INTELLIGENCE_VIEW'] },
        loadComponent: () =>
          import('./features/analytics/recommendations-list-page.component').then(
            (m) => m.RecommendationsListPageComponent,
          ),
      },
      {
        path: 'approvals',
        canActivate: [permissionGuard],
        data: { permissions: ['APPROVAL_REQUEST_VIEW'] },
        loadComponent: () =>
          import('./features/approvals/approvals-hub.component').then((m) => m.ApprovalsHubComponent),
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./features/approvals/approvals-list-page.component').then((m) => m.ApprovalsListPageComponent),
          },
          {
            path: 'submitted',
            loadComponent: () =>
              import('./features/approvals/approvals-list-page.component').then((m) => m.ApprovalsListPageComponent),
          },
          {
            path: 'assigned',
            loadComponent: () =>
              import('./features/approvals/approvals-list-page.component').then((m) => m.ApprovalsListPageComponent),
          },
          {
            path: 'create',
            canActivate: [permissionGuard],
            data: { permissions: ['APPROVAL_REQUEST_CREATE'] },
            loadComponent: () =>
              import('./features/approvals/approval-create-page.component').then((m) => m.ApprovalCreatePageComponent),
          },
          {
            path: ':id',
            canActivate: [permissionGuard],
            data: { permissions: ['APPROVAL_REQUEST_VIEW'] },
            loadComponent: () =>
              import('./features/approvals/approval-detail-page.component').then((m) => m.ApprovalDetailPageComponent),
          },
        ],
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications-page.component').then((m) => m.NotificationsPageComponent),
      },
      {
        path: 'notification-templates',
        canActivate: [permissionGuard],
        data: { permissions: ['NOTIFICATION_VIEW'] },
        loadComponent: () =>
          import('./features/notifications/notification-templates-page.component').then(
            (m) => m.NotificationTemplatesPageComponent,
          ),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings-page.component').then((m) => m.SettingsPageComponent),
      },
      {
        path: 'settings/system',
        loadComponent: () =>
          import('./features/settings/settings-system-page.component').then((m) => m.SettingsSystemPageComponent),
      },
      {
        path: 'settings/classification-rule-sets',
        loadComponent: () =>
          import('./features/settings/classification-rule-sets-page.component').then(
            (m) => m.ClassificationRuleSetsPageComponent,
          ),
      },
      {
        path: 'settings/executive-analytics',
        loadComponent: () =>
          import('./features/settings/settings-executive-analytics-page.component').then(
            (m) => m.SettingsExecutiveAnalyticsPageComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '/dashboard' },
];
