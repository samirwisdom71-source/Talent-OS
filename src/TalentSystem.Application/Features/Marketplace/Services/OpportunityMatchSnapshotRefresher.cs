using Microsoft.EntityFrameworkCore;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;
using TalentSystem.Persistence;

namespace TalentSystem.Application.Features.Marketplace.Services;

/// <summary>
/// Rules-based match snapshot (no AI). Upserted when an employee applies.
/// </summary>
internal static class OpportunityMatchSnapshotRefresher
{
    public static (decimal MatchScore, OpportunityMatchLevel MatchLevel, string? Notes) ComputeMatch(
        MarketplaceOpportunity opportunity,
        Employee employee)
    {
        var score = 15m;
        if (employee.OrganizationUnitId == opportunity.OrganizationUnitId)
        {
            score += 40m;
        }

        if (opportunity.PositionId is { } targetPositionId && employee.PositionId == targetPositionId)
        {
            score += 40m;
        }

        score = Math.Min(100m, Math.Round(score, 2, MidpointRounding.AwayFromZero));

        var level = score < 45m
            ? OpportunityMatchLevel.Low
            : score < 75m
                ? OpportunityMatchLevel.Medium
                : OpportunityMatchLevel.High;

        return (score, level, "Rules-based alignment (organization unit and target role).");
    }

    public static async Task UpsertAsync(
        TalentDbContext db,
        Guid marketplaceOpportunityId,
        Guid employeeId,
        MarketplaceOpportunity opportunity,
        Employee employee,
        CancellationToken cancellationToken)
    {
        var (matchScore, matchLevel, notes) = ComputeMatch(opportunity, employee);

        var existing = await db.OpportunityMatchSnapshots
            .FirstOrDefaultAsync(
                x => x.MarketplaceOpportunityId == marketplaceOpportunityId && x.EmployeeId == employeeId,
                cancellationToken);

        var utc = DateTime.UtcNow;
        if (existing is null)
        {
            db.OpportunityMatchSnapshots.Add(new OpportunityMatchSnapshot
            {
                MarketplaceOpportunityId = marketplaceOpportunityId,
                EmployeeId = employeeId,
                MatchScore = matchScore,
                MatchLevel = matchLevel,
                Notes = notes,
                CalculatedOnUtc = utc
            });
        }
        else
        {
            existing.MatchScore = matchScore;
            existing.MatchLevel = matchLevel;
            existing.Notes = notes;
            existing.CalculatedOnUtc = utc;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
