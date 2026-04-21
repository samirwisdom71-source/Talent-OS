using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.JobArchitecture.Interfaces;

public interface IJobGradeService
{
    Task<Result<JobGradeDto>> CreateAsync(
        CreateJobGradeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<JobGradeDto>> UpdateAsync(
        Guid id,
        UpdateJobGradeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<JobGradeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<JobGradeDto>>> GetPagedAsync(
        JobGradeFilterRequest request,
        CancellationToken cancellationToken = default);
}
