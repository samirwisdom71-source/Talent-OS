using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Approvals.DTOs;

public sealed class CreateApprovalRequestRequest
{
    public ApprovalRequestType RequestType { get; set; }

    public Guid RelatedEntityId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Notes { get; set; }
}

public sealed class UpdateApprovalRequestRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApprovalRequestDto
{
    public Guid Id { get; init; }

    public ApprovalRequestType RequestType { get; init; }

    public Guid RelatedEntityId { get; init; }

    public Guid RequestedByUserId { get; init; }

    public Guid? CurrentApproverUserId { get; init; }

    public ApprovalRequestStatus Status { get; init; }

    public DateTime? SubmittedOnUtc { get; init; }

    public DateTime? CompletedOnUtc { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<ApprovalActionDto> Actions { get; init; } = Array.Empty<ApprovalActionDto>();

    public IReadOnlyList<ApprovalAssignmentDto> Assignments { get; init; } = Array.Empty<ApprovalAssignmentDto>();
}

public sealed class ApprovalActionDto
{
    public Guid Id { get; init; }

    public ApprovalActionType ActionType { get; init; }

    public Guid ActionByUserId { get; init; }

    public string? Comments { get; init; }

    public DateTime ActionedOnUtc { get; init; }
}

public sealed class ApprovalAssignmentDto
{
    public Guid Id { get; init; }

    public Guid AssignedToUserId { get; init; }

    public Guid AssignedByUserId { get; init; }

    public DateTime AssignedOnUtc { get; init; }

    public bool IsCurrent { get; init; }

    public string? Notes { get; init; }
}

public sealed class ApprovalRequestListItemDto
{
    public Guid Id { get; init; }

    public ApprovalRequestType RequestType { get; init; }

    public Guid RelatedEntityId { get; init; }

    public Guid RequestedByUserId { get; init; }

    public Guid? CurrentApproverUserId { get; init; }

    public ApprovalRequestStatus Status { get; init; }

    public DateTime? SubmittedOnUtc { get; init; }

    public string Title { get; init; } = string.Empty;
}

public sealed class ApprovalRequestFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public ApprovalRequestStatus? Status { get; set; }

    public ApprovalRequestType? RequestType { get; set; }
}

public sealed class ApprovalAssignRequest
{
    public Guid ApproverUserId { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApprovalReassignRequest
{
    public Guid NewApproverUserId { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApprovalWorkflowCommentRequest
{
    public string? Comments { get; set; }
}
