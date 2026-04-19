using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Interfaces;

public interface IJobCompetencyRequirementService
{
    Task<Result<JobCompetencyRequirementDto>> CreateAsync(
        CreateJobCompetencyRequirementRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<JobCompetencyRequirementDto>> UpdateAsync(
        Guid id,
        UpdateJobCompetencyRequirementRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<JobCompetencyRequirementDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<JobCompetencyRequirementDto>>> GetPagedAsync(
        JobCompetencyRequirementFilterRequest request,
        CancellationToken cancellationToken = default);
}
