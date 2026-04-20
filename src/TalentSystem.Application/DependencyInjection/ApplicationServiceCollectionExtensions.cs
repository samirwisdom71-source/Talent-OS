using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TalentSystem.Application.Features.Competencies.Interfaces;
using TalentSystem.Application.Features.Competencies.Services;
using TalentSystem.Application.Features.Employees.Interfaces;
using TalentSystem.Application.Features.Employees.Services;
using TalentSystem.Application.Features.Employees.Validators;
using TalentSystem.Application.Features.Performance.Interfaces;
using TalentSystem.Application.Features.Performance.Services;
using TalentSystem.Application.Features.Potential.Interfaces;
using TalentSystem.Application.Features.Potential.Services;
using TalentSystem.Application.Features.Classification.Interfaces;
using TalentSystem.Application.Features.Classification.Services;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Application.Features.Development.Services;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Application.Features.Marketplace.Services;
using TalentSystem.Application.Features.Succession.Interfaces;
using TalentSystem.Application.Features.Succession.Services;
using TalentSystem.Application.Features.Scoring.Interfaces;
using TalentSystem.Application.Features.Scoring.Services;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Application.Features.Analytics.Services;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Application.Features.Identity.Services;
using TalentSystem.Application.Features.Approvals.Interfaces;
using TalentSystem.Application.Features.Approvals.Services;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Application.Features.Notifications.Services;
using TalentSystem.Application.Features.Intelligence.Interfaces;
using TalentSystem.Application.Features.Intelligence.Services;

namespace TalentSystem.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateEmployeeRequestValidator>();

        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddScoped<ICompetencyCategoryService, CompetencyCategoryService>();
        services.AddScoped<ICompetencyService, CompetencyService>();
        services.AddScoped<ICompetencyLevelService, CompetencyLevelService>();
        services.AddScoped<IJobCompetencyRequirementService, JobCompetencyRequirementService>();

        services.AddScoped<IPerformanceCycleService, PerformanceCycleService>();
        services.AddScoped<IPerformanceGoalService, PerformanceGoalService>();
        services.AddScoped<IPerformanceEvaluationService, PerformanceEvaluationService>();

        services.AddScoped<IPotentialAssessmentService, PotentialAssessmentService>();

        services.AddScoped<ITalentScoreService, TalentScoreService>();
        services.AddScoped<IScoringPolicyService, ScoringPolicyService>();

        services.AddScoped<ITalentClassificationService, TalentClassificationService>();
        services.AddScoped<IClassificationRuleSetService, ClassificationRuleSetService>();

        services.AddScoped<ICriticalPositionService, CriticalPositionService>();
        services.AddScoped<ISuccessionPlanService, SuccessionPlanService>();
        services.AddScoped<ISuccessorCandidateService, SuccessorCandidateService>();

        services.AddScoped<IDevelopmentPlanService, DevelopmentPlanService>();
        services.AddScoped<IDevelopmentPlanItemService, DevelopmentPlanItemService>();

        services.AddScoped<IMarketplaceOpportunityService, MarketplaceOpportunityService>();
        services.AddScoped<IOpportunityApplicationService, OpportunityApplicationService>();

        services.AddScoped<IExecutiveAnalyticsService, ExecutiveAnalyticsService>();
        services.AddScoped<ITalentAnalyticsService, TalentAnalyticsService>();
        services.AddScoped<ISuccessionAnalyticsService, SuccessionAnalyticsService>();
        services.AddScoped<IDevelopmentAnalyticsService, DevelopmentAnalyticsService>();
        services.AddScoped<IMarketplaceAnalyticsService, MarketplaceAnalyticsService>();
        services.AddScoped<IPerformanceAnalyticsService, PerformanceAnalyticsService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IIdentityLookupService, IdentityLookupService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIdentityDatabaseSeeder, IdentityDatabaseSeeder>();

        services.AddScoped<IApprovalRequestService, ApprovalRequestService>();

        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<IIntelligenceProvider, RulesBasedIntelligenceProvider>();
        services.AddScoped<IIntelligenceService, IntelligenceService>();

        return services;
    }
}
