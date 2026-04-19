using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Succession.Interfaces;

public interface ISuccessorCandidateService
{
    Task<Result<SuccessorCandidateDto>> AddAsync(
        CreateSuccessorCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SuccessorCandidateDto>> UpdateAsync(
        Guid id,
        UpdateSuccessorCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SuccessorCandidateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<SuccessorCandidateDto>>> GetPagedAsync(
        SuccessorCandidateFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<SuccessorCandidateDto>> MarkPrimaryAsync(Guid id, CancellationToken cancellationToken = default);
}
