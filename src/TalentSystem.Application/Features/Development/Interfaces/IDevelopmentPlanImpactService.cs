using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Interfaces;

public interface IDevelopmentPlanImpactService
{
    Task<Result<IReadOnlyList<DevelopmentPlanImpactSnapshotDto>>> ListAsync(
        Guid planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// يحسب مؤشرات من بيانات النظام (درجة مواهب، أداء، إمكانات) ويخزن/يحدّث لقطة للمرحلة.
    /// </summary>
    Task<Result<DevelopmentPlanImpactSnapshotDto>> ComputeAndPersistAsync(
        Guid planId,
        DevelopmentImpactPhase phase,
        CancellationToken cancellationToken = default);
}
