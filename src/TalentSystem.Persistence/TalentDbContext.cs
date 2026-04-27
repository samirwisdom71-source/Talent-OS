using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    private bool _isCreatingSystemNotifications;

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

    public DbSet<DevelopmentPlanItemPath> DevelopmentPlanItemPaths => Set<DevelopmentPlanItemPath>();

    public DbSet<DevelopmentPlanItemPathHelper> DevelopmentPlanItemPathHelpers => Set<DevelopmentPlanItemPathHelper>();

    public DbSet<DevelopmentPlanImpactSnapshot> DevelopmentPlanImpactSnapshots => Set<DevelopmentPlanImpactSnapshot>();

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

        var pendingEvents = CollectNotificationEvents();
        var affected = await base.SaveChangesAsync(cancellationToken);

        if (!_isCreatingSystemNotifications)
        {
            await CreateSystemNotificationsAsync(pendingEvents, userId, utcNow, cancellationToken).ConfigureAwait(false);
        }

        return affected;
    }

    private IReadOnlyList<PendingNotificationEvent> CollectNotificationEvents()
    {
        if (_isCreatingSystemNotifications)
        {
            return [];
        }

        var list = new List<PendingNotificationEvent>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.Entity is Notification or NotificationTemplate or NotificationDispatchLog)
            {
                continue;
            }

            var state = entry.State;
            if (state is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            var action = ResolveAction(entry, state);
            if (action is null)
            {
                continue;
            }

            var relatedEmployeeId = TryGetGuidProperty(entry.Entity, "EmployeeId");
            var candidateUserIds = CollectCandidateUserIds(entry.Entity);
            list.Add(new PendingNotificationEvent(
                entry.Entity.Id,
                entry.Entity.GetType().Name,
                action.Value.title,
                action.Value.message,
                relatedEmployeeId,
                candidateUserIds));
        }

        return list;
    }

    private async Task CreateSystemNotificationsAsync(
        IReadOnlyList<PendingNotificationEvent> pendingEvents,
        Guid? actorUserId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (pendingEvents.Count == 0)
        {
            return;
        }

        var employeeIds = pendingEvents
            .Where(e => e.RelatedEmployeeId is not null && e.RelatedEmployeeId != Guid.Empty)
            .Select(e => e.RelatedEmployeeId!.Value)
            .Distinct()
            .ToList();

        var employeeToUserMap = employeeIds.Count == 0
            ? new Dictionary<Guid, Guid>()
            : await Users.AsNoTracking()
                .Where(u => u.IsActive && u.EmployeeId.HasValue && employeeIds.Contains(u.EmployeeId.Value))
                .OrderBy(u => u.CreatedOnUtc)
                .GroupBy(u => u.EmployeeId!.Value)
                .Select(g => new { EmployeeId = g.Key, UserId = g.Select(x => x.Id).FirstOrDefault() })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.UserId, cancellationToken)
                .ConfigureAwait(false);

        var toInsert = new List<Notification>();
        foreach (var evt in pendingEvents)
        {
            var recipientIds = new HashSet<Guid>();
            if (actorUserId is { } actor && actor != Guid.Empty)
            {
                recipientIds.Add(actor);
            }

            if (evt.RelatedEmployeeId is { } employeeId &&
                employeeToUserMap.TryGetValue(employeeId, out var employeeUserId) &&
                employeeUserId != Guid.Empty)
            {
                recipientIds.Add(employeeUserId);
            }

            foreach (var candidateUserId in evt.CandidateUserIds)
            {
                if (candidateUserId != Guid.Empty)
                {
                    recipientIds.Add(candidateUserId);
                }
            }

            foreach (var recipientId in recipientIds)
            {
                var notification = new Notification
                {
                    UserId = recipientId,
                    NotificationType = NotificationType.General,
                    Title = evt.Title,
                    Message = evt.Message,
                    Channel = NotificationChannel.InApp,
                    IsRead = false,
                    RelatedEntityId = evt.RelatedEntityId,
                    RelatedEntityType = evt.RelatedEntityType,
                    RecordStatus = RecordStatus.Active
                };

                notification.DispatchLogs.Add(new NotificationDispatchLog
                {
                    Channel = NotificationChannel.InApp,
                    DispatchStatus = NotificationDispatchStatus.Sent,
                    AttemptedOnUtc = utcNow,
                    RecordStatus = RecordStatus.Active
                });

                toInsert.Add(notification);
            }
        }

        if (toInsert.Count == 0)
        {
            return;
        }

        _isCreatingSystemNotifications = true;
        try
        {
            Notifications.AddRange(toInsert);
            await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _isCreatingSystemNotifications = false;
        }
    }

    private static (string title, string message)? ResolveAction(EntityEntry<AuditableEntity> entry, EntityState state)
    {
        var entityName = entry.Entity.GetType().Name;
        if (state == EntityState.Added)
        {
            return ($"{entityName} created", $"{entityName} was created successfully.");
        }

        if (state != EntityState.Modified)
        {
            return null;
        }

        if (entry.Entity is AuditableDomainEntity auditableDomainEntity &&
            auditableDomainEntity.RecordStatus == RecordStatus.Deleted)
        {
            return ($"{entityName} deleted", $"{entityName} was deleted.");
        }

        return ($"{entityName} updated", $"{entityName} was updated.");
    }

    private static Guid? TryGetGuidProperty(object entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property is null)
        {
            return null;
        }

        var value = property.GetValue(entity);
        if (value is Guid g && g != Guid.Empty)
        {
            return g;
        }

        return null;
    }

    private static IReadOnlyList<Guid> CollectCandidateUserIds(object entity)
    {
        var names = new[]
        {
            "UserId",
            "RequestedByUserId",
            "CurrentApproverUserId",
            "AssignedToUserId",
            "AssignedByUserId",
            "ActionByUserId",
            "CreatedByUserId",
            "ModifiedByUserId"
        };

        var values = new HashSet<Guid>();
        foreach (var name in names)
        {
            var value = TryGetGuidProperty(entity, name);
            if (value is { } id && id != Guid.Empty)
            {
                values.Add(id);
            }
        }

        return values.ToList();
    }

    private sealed record PendingNotificationEvent(
        Guid RelatedEntityId,
        string RelatedEntityType,
        string Title,
        string Message,
        Guid? RelatedEmployeeId,
        IReadOnlyList<Guid> CandidateUserIds);

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
