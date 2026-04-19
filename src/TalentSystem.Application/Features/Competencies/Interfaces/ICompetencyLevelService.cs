using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Interfaces;

public interface ICompetencyLevelService
{
    Task<Result<CompetencyLevelDto>> CreateAsync(
        CreateCompetencyLevelRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyLevelDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyLevelRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyLevelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CompetencyLevelDto>>> GetPagedAsync(
        CompetencyLevelFilterRequest request,
        CancellationToken cancellationToken = default);
}
