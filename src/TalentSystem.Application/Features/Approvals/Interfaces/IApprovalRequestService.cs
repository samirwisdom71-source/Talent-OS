using TalentSystem.Application.Features.Approvals.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Approvals.Interfaces;

public interface IApprovalRequestService
{
    Task<Result<ApprovalRequestDto>> CreateDraftAsync(CreateApprovalRequestRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> UpdateDraftAsync(Guid id, UpdateApprovalRequestRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetPagedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetMySubmittedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetMyAssignedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> CancelAsync(Guid id, ApprovalWorkflowCommentRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> AssignApproverAsync(Guid id, ApprovalAssignRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> ReassignApproverAsync(Guid id, ApprovalReassignRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> StartReviewAsync(Guid id, ApprovalWorkflowCommentRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> ApproveAsync(Guid id, ApprovalWorkflowCommentRequest request, CancellationToken cancellationToken = default);

    Task<Result<ApprovalRequestDto>> RejectAsync(Guid id, ApprovalWorkflowCommentRequest request, CancellationToken cancellationToken = default);
}
