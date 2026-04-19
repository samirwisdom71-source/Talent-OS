using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Scoring.Interfaces;

public interface IScoringPolicyService
{
    Task<Result<ScoringPolicyDto>> CreateAsync(
        CreateScoringPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ScoringPolicyDto>> UpdateAsync(
        Guid id,
        UpdateScoringPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ScoringPolicyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ScoringPolicyDto>>> GetPagedAsync(
        ScoringPolicyFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ScoringPolicyDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
