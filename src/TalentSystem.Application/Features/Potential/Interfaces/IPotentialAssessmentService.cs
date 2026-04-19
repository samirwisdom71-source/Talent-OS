using TalentSystem.Application.Features.Potential.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Potential.Interfaces;

public interface IPotentialAssessmentService
{
    Task<Result<PotentialAssessmentDto>> CreateAsync(
        CreatePotentialAssessmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PotentialAssessmentDto>> UpdateAsync(
        Guid id,
        UpdatePotentialAssessmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PotentialAssessmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PotentialAssessmentDto>>> GetPagedAsync(
        PotentialAssessmentFilterRequest request,
        CancellationToken cancellationToken = default);
}
