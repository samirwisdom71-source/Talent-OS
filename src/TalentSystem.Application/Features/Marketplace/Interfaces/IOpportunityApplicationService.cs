using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Marketplace.Interfaces;

public interface IOpportunityApplicationService
{
    Task<Result<OpportunityApplicationDto>> ApplyAsync(
        ApplyOpportunityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> UpdateAsync(
        Guid id,
        UpdateOpportunityApplicationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<OpportunityApplicationDto>>> GetPagedAsync(
        OpportunityApplicationFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> WithdrawAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> MarkUnderReviewAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> ShortlistAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> AcceptAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OpportunityApplicationDto>> RejectAsync(Guid id, CancellationToken cancellationToken = default);
}
