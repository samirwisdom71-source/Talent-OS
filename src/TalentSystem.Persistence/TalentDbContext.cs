using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Domain.Common;
using TalentSystem.Domain.Competencies;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.JobArchitecture;
using TalentSystem.Domain.Organizations;
using TalentSystem.Domain.Performance;
using TalentSystem.Domain.Potential;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Marketplace;
using TalentSystem.Domain.Scoring;
using TalentSystem.Domain.Succession;
using TalentSystem.Domain.Talent;
using TalentSystem.Domain.Identity;
using TalentSystem.Domain.Approvals;
using TalentSystem.Domain.Notifications;
using TalentSystem.Domain.Intelligence;
using TalentSystem.Shared.Abstractions;
using TalentSystem.Shared.Identity;

namespace TalentSystem.Persistence;

public sealed class TalentDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public TalentDbContext(DbContextOptions<TalentDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();

    public DbSet<JobGrade> JobGrades => Set<JobGrade>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<TalentProfile> TalentProfiles => Set<TalentProfile>();

    public DbSet<CompetencyCategory> CompetencyCategories => Set<CompetencyCategory>();

    public DbSet<Competency> Competencies => Set<Competency>();

    public DbSet<CompetencyLevel> CompetencyLevels => Set<CompetencyLevel>();

    public DbSet<JobCompetencyRequirement> JobCompetencyRequirements => Set<JobCompetencyRequirement>();

    public DbSet<PerformanceCycle> PerformanceCycles => Set<PerformanceCycle>();

    public DbSet<PerformanceGoal> PerformanceGoals => Set<PerformanceGoal>();

    public DbSet<PerformanceEvaluation> PerformanceEvaluations => Set<PerformanceEvaluation>();

    public DbSet<PotentialAssessment> PotentialAssessments => Set<PotentialAssessment>();

    public DbSet<PotentialAssessmentFactor> PotentialAssessmentFactors => Set<PotentialAssessmentFactor>();

    public DbSet<TalentScore> TalentScores => Set<TalentScore>();

    public DbSet<ScoringPolicy> ScoringPolicies => Set<ScoringPolicy>();

    public DbSet<TalentClassification> TalentClassifications => Set<TalentClassification>();

    public DbSet<ClassificationRuleSet> ClassificationRuleSets => Set<ClassificationRuleSet>();

    public DbSet<CriticalPosition> CriticalPositions => Set<CriticalPosition>();

    public DbSet<SuccessionPlan> SuccessionPlans => Set<SuccessionPlan>();

    public DbSet<SuccessorCandidate> SuccessorCandidates => Set<SuccessorCandidate>();

    public DbSet<SuccessionCoverageSnapshot> SuccessionCoverageSnapshots => Set<SuccessionCoverageSnapshot>();

    public DbSet<DevelopmentPlan> DevelopmentPlans => Set<DevelopmentPlan>();

    public DbSet<DevelopmentPlanItem> DevelopmentPlanItems => Set<DevelopmentPlanItem>();

    public DbSet<DevelopmentPlanLink> DevelopmentPlanLinks => Set<DevelopmentPlanLink>();

    public DbSet<MarketplaceOpportunity> MarketplaceOpportunities => Set<MarketplaceOpportunity>();

    public DbSet<OpportunityApplication> OpportunityApplications => Set<OpportunityApplication>();

    public DbSet<OpportunityMatchSnapshot> OpportunityMatchSnapshots => Set<OpportunityMatchSnapshot>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();

    public DbSet<ApprovalAssignment> ApprovalAssignments => Set<ApprovalAssignment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<NotificationDispatchLog> NotificationDispatchLogs => Set<NotificationDispatchLog>();

    public DbSet<TalentInsight> TalentInsights => Set<TalentInsight>();

    public DbSet<TalentRecommendation> TalentRecommendations => Set<TalentRecommendation>();

    public DbSet<IntelligenceRun> IntelligenceRuns => Set<IntelligenceRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TalentDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.CreateVersion7();
                    }

                    entry.Entity.CreatedOnUtc = utcNow;
                    entry.Entity.CreatedByUserId = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedOnUtc = utcNow;
                    entry.Entity.ModifiedByUserId = userId;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuditableDomainEntity>())
        {
            if (entry.State == EntityState.Modified &&
                entry.Entity.RecordStatus == RecordStatus.Deleted &&
                entry.Entity.DeletedOnUtc is null)
            {
                entry.Entity.DeletedOnUtc = utcNow;
                entry.Entity.DeletedByUserId = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null || !typeof(AuditableDomainEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            var method = typeof(TalentDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(clrType);

            method.Invoke(null, new object[] { modelBuilder });
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableDomainEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.RecordStatus != RecordStatus.Deleted);
    }
}
