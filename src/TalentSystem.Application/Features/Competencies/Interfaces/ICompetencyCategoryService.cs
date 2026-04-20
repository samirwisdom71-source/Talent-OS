using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Interfaces;

public interface ICompetencyCategoryService
{
    Task<Result<CompetencyCategoryDto>> CreateAsync(
        CreateCompetencyCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyCategoryDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CompetencyCategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CompetencyCategoryDto>>> GetPagedAsync(
        CompetencyCategoryFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetLookupAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);
}
