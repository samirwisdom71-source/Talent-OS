using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class ExecutiveAnalyticsService : IExecutiveAnalyticsService
{
    private readonly TalentDbContext _db;

    public ExecutiveAnalyticsService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ExecutiveDashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // EF Core DbContext does not support concurrent operations; await counts sequentially.
        var dto = new ExecutiveDashboardSummaryDto
        {
            TotalEmployees = await _db.Employees.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            TotalTalentScores = await _db.TalentScores.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            TotalTalentClassifications = await _db.TalentClassifications.AsNoTracking().CountAsync(cancellationToken)
                .ConfigureAwait(false),
            HighPotentialCount = await _db.TalentClassifications.AsNoTracking()
                .CountAsync(tc => tc.IsHighPotential, cancellationToken).ConfigureAwait(false),
            HighPerformerCount = await _db.TalentClassifications.AsNoTracking()
                .CountAsync(tc => tc.IsHighPerformer, cancellationToken).ConfigureAwait(false),
            StrategicLeaderCount = await _db.TalentClassifications.AsNoTracking()
                .CountAsync(tc => tc.NineBoxCode == NineBoxCode.Box9, cancellationToken).ConfigureAwait(false),
            ActivePerformanceCycleCount = await _db.PerformanceCycles.AsNoTracking()
                .CountAsync(c => c.Status == PerformanceCycleStatus.Active, cancellationToken).ConfigureAwait(false),
            ActiveSuccessionPlanCount = await _db.SuccessionPlans.AsNoTracking()
                .CountAsync(p => p.Status == SuccessionPlanStatus.Active, cancellationToken).ConfigureAwait(false),
            OpenMarketplaceOpportunityCount = await _db.MarketplaceOpportunities.AsNoTracking()
                .CountAsync(o => o.Status == MarketplaceOpportunityStatus.Open, cancellationToken).ConfigureAwait(false),
            ActiveDevelopmentPlanCount = await _db.DevelopmentPlans.AsNoTracking()
                .CountAsync(p => p.Status == DevelopmentPlanStatus.Active, cancellationToken).ConfigureAwait(false)
        };

        return Result<ExecutiveDashboardSummaryDto>.Ok(dto);
    }
}
