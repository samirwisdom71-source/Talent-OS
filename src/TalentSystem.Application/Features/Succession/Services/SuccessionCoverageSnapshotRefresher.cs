using Microsoft.EntityFrameworkCore;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;
using TalentSystem.Persistence;

namespace TalentSystem.Application.Features.Succession.Services;

internal static class SuccessionCoverageSnapshotRefresher
{
    /// <summary>
    /// Upserts the single coverage snapshot for a plan after candidate changes.
    /// Score blends candidate depth, readiness-now, and designated primary (0–100, capped).
    /// </summary>
    public static async Task RefreshAsync(
        TalentDbContext db,
        Guid successionPlanId,
        CancellationToken cancellationToken)
    {
        var candidates = await db.SuccessorCandidates
            .Where(x => x.SuccessionPlanId == successionPlanId)
            .ToListAsync(cancellationToken);

        var total = candidates.Count;
        var hasReadyNow = candidates.Any(x => x.ReadinessLevel == ReadinessLevel.ReadyNow);
        var hasPrimarySuccessor = candidates.Any(x => x.IsPrimarySuccessor);
        var score = CalculateCoverageScore(total, hasReadyNow, hasPrimarySuccessor);

        var existing = await db.SuccessionCoverageSnapshots
            .FirstOrDefaultAsync(x => x.SuccessionPlanId == successionPlanId, cancellationToken);

        var utc = DateTime.UtcNow;
        if (existing is null)
        {
            db.SuccessionCoverageSnapshots.Add(new SuccessionCoverageSnapshot
            {
                SuccessionPlanId = successionPlanId,
                TotalCandidates = total,
                HasReadyNow = hasReadyNow,
                HasPrimarySuccessor = hasPrimarySuccessor,
                CoverageScore = score,
                CalculatedOnUtc = utc
            });
        }
        else
        {
            existing.TotalCandidates = total;
            existing.HasReadyNow = hasReadyNow;
            existing.HasPrimarySuccessor = hasPrimarySuccessor;
            existing.CoverageScore = score;
            existing.CalculatedOnUtc = utc;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static decimal CalculateCoverageScore(int total, bool hasReadyNow, bool hasPrimarySuccessor)
    {
        if (total <= 0)
        {
            return 0m;
        }

        var raw = total * 12m;
        if (hasReadyNow)
        {
            raw += 28m;
        }

        if (hasPrimarySuccessor)
        {
            raw += 28m;
        }

        return Math.Min(100m, Math.Round(raw, 2, MidpointRounding.AwayFromZero));
    }
}
