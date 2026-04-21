using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Interfaces;

public interface ICompetencyService
{
    Task<Result<CompetencyDto>> CreateAsync(
        CreateCompetencyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CompetencyDto>>> GetPagedAsync(
        CompetencyFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetLookupAsync(
        CompetencyLookupRequest request,
        CancellationToken cancellationToken = default);
}
