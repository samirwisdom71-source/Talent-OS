using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Approvals.DTOs;
using TalentSystem.Application.Features.Approvals.Interfaces;
using TalentSystem.Application.Features.Approvals.Validators;
using TalentSystem.Application.Features.Notifications.Integration;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Domain.Approvals;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Identity;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Approvals.Services;

public sealed class ApprovalRequestService : IApprovalRequestService
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateApprovalRequestRequest> _createValidator;
    private readonly IValidator<UpdateApprovalRequestRequest> _updateValidator;
    private readonly IValidator<ApprovalRequestFilterRequest> _filterValidator;
    private readonly IValidator<ApprovalAssignRequest> _assignValidator;
    private readonly IValidator<ApprovalReassignRequest> _reassignValidator;
    private readonly IValidator<ApprovalWorkflowCommentRequest> _commentValidator;
    private readonly INotificationService _notifications;

    public ApprovalRequestService(
        TalentDbContext db,
        ICurrentUserService currentUser,
        IValidator<CreateApprovalRequestRequest> createValidator,
        IValidator<UpdateApprovalRequestRequest> updateValidator,
        IValidator<ApprovalRequestFilterRequest> filterValidator,
        IValidator<ApprovalAssignRequest> assignValidator,
        IValidator<ApprovalReassignRequest> reassignValidator,
        IValidator<ApprovalWorkflowCommentRequest> commentValidator,
        INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _assignValidator = assignValidator;
        _reassignValidator = reassignValidator;
        _commentValidator = commentValidator;
        _notifications = notifications;
    }

    public async Task<Result<ApprovalRequestDto>> CreateDraftAsync(
        CreateApprovalRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var requesterId = userIdResult.Value;
        if (!await UserExistsAndActiveAsync(requesterId, cancellationToken).ConfigureAwait(false))
        {
            return Result<ApprovalRequestDto>.Fail(
                "The current user was not found or is inactive.",
                ApprovalErrors.RequestedUserNotFound);
        }

        var entity = new ApprovalRequest
        {
            RequestType = request.RequestType,
            RelatedEntityId = request.RelatedEntityId,
            RequestedByUserId = requesterId,
            Status = ApprovalRequestStatus.Draft,
            Title = request.Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordStatus = RecordStatus.Active
        };

        _db.ApprovalRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> UpdateDraftAsync(
        Guid id,
        UpdateApprovalRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.RequestedByUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the requester can update this draft.",
                ApprovalErrors.NotRequester);
        }

        if (entity.Status != ApprovalRequestStatus.Draft)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only draft requests can be updated.",
                ApprovalErrors.DraftOnly);
        }

        entity.Title = request.Title.Trim();
        entity.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.RequestedByUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the requester can submit this request.",
                ApprovalErrors.NotRequester);
        }

        if (entity.Status != ApprovalRequestStatus.Draft)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only draft requests can be submitted.",
                ApprovalErrors.InvalidStatus);
        }

        var utc = DateTime.UtcNow;
        entity.Status = ApprovalRequestStatus.Submitted;
        entity.SubmittedOnUtc = utc;

        await AddActionAsync(entity.Id, userIdResult.Value, ApprovalActionType.Submit, null, utc, cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _db.ApprovalRequests.AsNoTracking().AnyAsync(a => a.Id == id, cancellationToken).ConfigureAwait(false))
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetPagedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        return await QueryPagedAsync(request, _db.ApprovalRequests.AsNoTracking(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetMySubmittedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<PagedResult<ApprovalRequestListItemDto>>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var query = _db.ApprovalRequests.AsNoTracking().Where(a => a.RequestedByUserId == userIdResult.Value);
        return await QueryPagedAsync(request, query, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Result<PagedResult<ApprovalRequestListItemDto>>> GetMyAssignedAsync(
        ApprovalRequestFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<PagedResult<ApprovalRequestListItemDto>>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var query = _db.ApprovalRequests.AsNoTracking()
            .Where(a => a.CurrentApproverUserId == userIdResult.Value);
        return await QueryPagedAsync(request, query, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Result<ApprovalRequestDto>> CancelAsync(
        Guid id,
        ApprovalWorkflowCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var val = await _commentValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!val.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(val.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.RequestedByUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the requester can cancel this request.",
                ApprovalErrors.NotRequester);
        }

        if (entity.Status is not (ApprovalRequestStatus.Submitted or ApprovalRequestStatus.InReview))
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only submitted or in-review requests can be cancelled.",
                ApprovalErrors.InvalidStatus);
        }

        var utc = DateTime.UtcNow;
        entity.Status = ApprovalRequestStatus.Cancelled;
        entity.CompletedOnUtc = utc;
        entity.CurrentApproverUserId = null;

        await CloseCurrentAssignmentsAsync(entity.Id, cancellationToken).ConfigureAwait(false);

        await AddActionAsync(
                entity.Id,
                userIdResult.Value,
                ApprovalActionType.Cancel,
                TrimOrNull(request.Comments),
                utc,
                cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> AssignApproverAsync(
        Guid id,
        ApprovalAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _assignValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var actorResult = RequireCurrentUserId();
        if (actorResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(actorResult.Errors, actorResult.FailureCode);
        }

        var approverCheck = await ValidateApproverAsync(request.ApproverUserId, cancellationToken).ConfigureAwait(false);
        if (approverCheck is not null)
        {
            return Result<ApprovalRequestDto>.Fail(approverCheck.Value.message, approverCheck.Value.code);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.Status is not (ApprovalRequestStatus.Submitted or ApprovalRequestStatus.InReview))
        {
            return Result<ApprovalRequestDto>.Fail(
                "Approver can only be assigned while the request is submitted or in review.",
                ApprovalErrors.InvalidStateForAssign);
        }

        if (entity.CurrentApproverUserId is not null)
        {
            return Result<ApprovalRequestDto>.Fail(
                "An approver is already assigned. Use reassign instead.",
                ApprovalErrors.InvalidStateForAssign);
        }

        var utc = DateTime.UtcNow;
        entity.CurrentApproverUserId = request.ApproverUserId;

        await AddAssignmentAsync(
                entity.Id,
                request.ApproverUserId,
                actorResult.Value,
                utc,
                TrimOrNull(request.Notes),
                cancellationToken)
            .ConfigureAwait(false);

        await AddActionAsync(
                entity.Id,
                actorResult.Value,
                ApprovalActionType.Assign,
                TrimOrNull(request.Notes),
                utc,
                cancellationToken)
            .ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _notifications.PublishIntegrationAsync(
                ApprovalNotificationRequests.ForAssigned(entity.Id, request.ApproverUserId, entity.Title),
                cancellationToken)
            .ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> ReassignApproverAsync(
        Guid id,
        ApprovalReassignRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _reassignValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var actorResult = RequireCurrentUserId();
        if (actorResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(actorResult.Errors, actorResult.FailureCode);
        }

        var approverCheck = await ValidateApproverAsync(request.NewApproverUserId, cancellationToken)
            .ConfigureAwait(false);
        if (approverCheck is not null)
        {
            return Result<ApprovalRequestDto>.Fail(approverCheck.Value.message, approverCheck.Value.code);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.Status is not (ApprovalRequestStatus.Submitted or ApprovalRequestStatus.InReview))
        {
            return Result<ApprovalRequestDto>.Fail(
                "Reassign is only allowed while the request is submitted or in review.",
                ApprovalErrors.InvalidStateForAssign);
        }

        if (entity.CurrentApproverUserId is null)
        {
            return Result<ApprovalRequestDto>.Fail(
                "There is no current approver to replace. Use assign instead.",
                ApprovalErrors.InvalidStateForAssign);
        }

        var utc = DateTime.UtcNow;
        entity.CurrentApproverUserId = request.NewApproverUserId;

        await CloseCurrentAssignmentsAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        await AddAssignmentAsync(
                entity.Id,
                request.NewApproverUserId,
                actorResult.Value,
                utc,
                TrimOrNull(request.Notes),
                cancellationToken)
            .ConfigureAwait(false);

        await AddActionAsync(
                entity.Id,
                actorResult.Value,
                ApprovalActionType.Reassign,
                TrimOrNull(request.Notes),
                utc,
                cancellationToken)
            .ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _notifications.PublishIntegrationAsync(
                ApprovalNotificationRequests.ForAssigned(entity.Id, request.NewApproverUserId, entity.Title),
                cancellationToken)
            .ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> StartReviewAsync(
        Guid id,
        ApprovalWorkflowCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var val = await _commentValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!val.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(val.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.Status != ApprovalRequestStatus.Submitted)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Review can only be started from the submitted state.",
                ApprovalErrors.InvalidStatus);
        }

        if (entity.CurrentApproverUserId is null)
        {
            return Result<ApprovalRequestDto>.Fail(
                "An approver must be assigned before review can start.",
                ApprovalErrors.InvalidStatus);
        }

        if (entity.CurrentApproverUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the current approver can start review.",
                ApprovalErrors.NotCurrentApprover);
        }

        var utc = DateTime.UtcNow;
        entity.Status = ApprovalRequestStatus.InReview;

        await AddActionAsync(
                entity.Id,
                userIdResult.Value,
                ApprovalActionType.StartReview,
                TrimOrNull(request.Comments),
                utc,
                cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> ApproveAsync(
        Guid id,
        ApprovalWorkflowCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var val = await _commentValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!val.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(val.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.Status != ApprovalRequestStatus.InReview)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only requests in review can be approved.",
                ApprovalErrors.InvalidStatus);
        }

        if (entity.CurrentApproverUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the current approver can approve.",
                ApprovalErrors.NotCurrentApprover);
        }

        var utc = DateTime.UtcNow;
        entity.Status = ApprovalRequestStatus.Approved;
        entity.CompletedOnUtc = utc;
        entity.CurrentApproverUserId = null;

        await CloseCurrentAssignmentsAsync(entity.Id, cancellationToken).ConfigureAwait(false);

        var requesterUserId = entity.RequestedByUserId;

        await AddActionAsync(
                entity.Id,
                userIdResult.Value,
                ApprovalActionType.Approve,
                TrimOrNull(request.Comments),
                utc,
                cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _notifications.PublishIntegrationAsync(
                ApprovalNotificationRequests.ForApproved(entity.Id, requesterUserId, entity.Title),
                cancellationToken)
            .ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<ApprovalRequestDto>> RejectAsync(
        Guid id,
        ApprovalWorkflowCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var val = await _commentValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!val.IsValid)
        {
            return Result<ApprovalRequestDto>.Fail(val.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userIdResult = RequireCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return Result<ApprovalRequestDto>.Fail(userIdResult.Errors, userIdResult.FailureCode);
        }

        var entity = await _db.ApprovalRequests.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ApprovalRequestDto>.Fail("The approval request was not found.", ApprovalErrors.NotFound);
        }

        if (entity.Status != ApprovalRequestStatus.InReview)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only requests in review can be rejected.",
                ApprovalErrors.InvalidStatus);
        }

        if (entity.CurrentApproverUserId != userIdResult.Value)
        {
            return Result<ApprovalRequestDto>.Fail(
                "Only the current approver can reject.",
                ApprovalErrors.NotCurrentApprover);
        }

        var utc = DateTime.UtcNow;
        entity.Status = ApprovalRequestStatus.Rejected;
        entity.CompletedOnUtc = utc;
        entity.CurrentApproverUserId = null;

        await CloseCurrentAssignmentsAsync(entity.Id, cancellationToken).ConfigureAwait(false);

        var requesterUserId = entity.RequestedByUserId;

        await AddActionAsync(
                entity.Id,
                userIdResult.Value,
                ApprovalActionType.Reject,
                TrimOrNull(request.Comments),
                utc,
                cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _notifications.PublishIntegrationAsync(
                ApprovalNotificationRequests.ForRejected(entity.Id, requesterUserId, entity.Title),
                cancellationToken)
            .ConfigureAwait(false);

        return Result<ApprovalRequestDto>.Ok(await MapDetailAsync(entity.Id, cancellationToken).ConfigureAwait(false));
    }

    private Result<Guid> RequireCurrentUserId()
    {
        if (_currentUser.UserId is not { } id || id == Guid.Empty)
        {
            return Result<Guid>.Fail("An authenticated user is required.", ApprovalErrors.CurrentUserRequired);
        }

        return Result<Guid>.Ok(id);
    }

    private static string? TrimOrNull(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private async Task<bool> UserExistsAndActiveAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken)
            .ConfigureAwait(false);

    private async Task<(string message, string code)?> ValidateApproverAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return ("The approver user was not found.", ApprovalErrors.ApproverNotFound);
        }

        if (!user.IsActive)
        {
            return ("The approver user is inactive.", ApprovalErrors.ApproverInactive);
        }

        return null;
    }

    private Task AddActionAsync(
        Guid approvalRequestId,
        Guid actionByUserId,
        ApprovalActionType actionType,
        string? comments,
        DateTime actionedOnUtc,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        _db.ApprovalActions.Add(new ApprovalAction
        {
            ApprovalRequestId = approvalRequestId,
            ActionByUserId = actionByUserId,
            ActionType = actionType,
            Comments = comments,
            ActionedOnUtc = actionedOnUtc,
            RecordStatus = RecordStatus.Active
        });
        return Task.CompletedTask;
    }

    private async Task CloseCurrentAssignmentsAsync(Guid approvalRequestId, CancellationToken cancellationToken)
    {
        var open = await _db.ApprovalAssignments
            .Where(a => a.ApprovalRequestId == approvalRequestId && a.IsCurrent)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var a in open)
        {
            a.IsCurrent = false;
        }
    }

    private async Task AddAssignmentAsync(
        Guid approvalRequestId,
        Guid assignedToUserId,
        Guid assignedByUserId,
        DateTime assignedOnUtc,
        string? notes,
        CancellationToken cancellationToken)
    {
        await CloseCurrentAssignmentsAsync(approvalRequestId, cancellationToken).ConfigureAwait(false);

        _db.ApprovalAssignments.Add(new ApprovalAssignment
        {
            ApprovalRequestId = approvalRequestId,
            AssignedToUserId = assignedToUserId,
            AssignedByUserId = assignedByUserId,
            AssignedOnUtc = assignedOnUtc,
            IsCurrent = true,
            Notes = notes,
            RecordStatus = RecordStatus.Active
        });
    }

    private async Task<Result<PagedResult<ApprovalRequestListItemDto>>> QueryPagedAsync(
        ApprovalRequestFilterRequest request,
        IQueryable<ApprovalRequest> query,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<PagedResult<ApprovalRequestListItemDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        if (request.Status is { } st)
        {
            query = query.Where(a => a.Status == st);
        }

        if (request.RequestType is { } rt)
        {
            query = query.Where(a => a.RequestType == rt);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(a => a.SubmittedOnUtc ?? a.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApprovalRequestListItemDto
            {
                Id = a.Id,
                RequestType = a.RequestType,
                RelatedEntityId = a.RelatedEntityId,
                RequestedByUserId = a.RequestedByUserId,
                CurrentApproverUserId = a.CurrentApproverUserId,
                Status = a.Status,
                SubmittedOnUtc = a.SubmittedOnUtc,
                Title = a.Title
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<ApprovalRequestListItemDto>>.Ok(new PagedResult<ApprovalRequestListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    private async Task<ApprovalRequestDto> MapDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.ApprovalRequests.AsNoTracking()
            .Include(a => a.Actions)
            .Include(a => a.Assignments)
            .FirstAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return new ApprovalRequestDto
        {
            Id = entity.Id,
            RequestType = entity.RequestType,
            RelatedEntityId = entity.RelatedEntityId,
            RequestedByUserId = entity.RequestedByUserId,
            CurrentApproverUserId = entity.CurrentApproverUserId,
            Status = entity.Status,
            SubmittedOnUtc = entity.SubmittedOnUtc,
            CompletedOnUtc = entity.CompletedOnUtc,
            Title = entity.Title,
            Summary = entity.Summary,
            Notes = entity.Notes,
            Actions = entity.Actions
                .OrderByDescending(x => x.ActionedOnUtc)
                .Select(x => new ApprovalActionDto
                {
                    Id = x.Id,
                    ActionType = x.ActionType,
                    ActionByUserId = x.ActionByUserId,
                    Comments = x.Comments,
                    ActionedOnUtc = x.ActionedOnUtc
                })
                .ToList(),
            Assignments = entity.Assignments
                .OrderByDescending(x => x.AssignedOnUtc)
                .Select(x => new ApprovalAssignmentDto
                {
                    Id = x.Id,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedByUserId = x.AssignedByUserId,
                    AssignedOnUtc = x.AssignedOnUtc,
                    IsCurrent = x.IsCurrent,
                    Notes = x.Notes
                })
                .ToList()
        };
    }
}
