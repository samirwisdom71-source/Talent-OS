using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Scoring.Interfaces;

public interface ITalentScoreService
{
    Task<Result<TalentScoreDto>> CalculateAsync(
        CalculateTalentScoreRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TalentScoreDto>> RecalculateAsync(
        RecalculateTalentScoreRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TalentScoreDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<TalentScoreDto>> GetByEmployeeAndCycleAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<TalentScoreDto>>> GetPagedAsync(
        TalentScoreFilterRequest request,
        CancellationToken cancellationToken = default);
}
