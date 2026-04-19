using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Approvals.DTOs;
using TalentSystem.Application.Features.Approvals.Interfaces;
using TalentSystem.Application.Features.Identity;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/approval-requests")]
[Authorize]
public sealed class ApprovalRequestsController : ControllerBase
{
    private readonly IApprovalRequestService _approvalRequestService;

    public ApprovalRequestsController(IApprovalRequestService approvalRequestService)
    {
        _approvalRequestService = approvalRequestService;
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ApprovalRequestCreate)]
    public async Task<IActionResult> Create([FromBody] CreateApprovalRequestRequest request, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.CreateDraftAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
                ApiResponse<ApprovalRequestDto>.FromSuccess(result.Value, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestCreate)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateApprovalRequestRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.UpdateDraftAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestCreate)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.SubmitAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.GetByIdAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.ApprovalRequestView)]
    public async Task<IActionResult> GetPaged([FromQuery] ApprovalRequestFilterRequest request, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.GetPagedAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<ApprovalRequestListItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("my-submitted")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestView)]
    public async Task<IActionResult> GetMySubmitted([FromQuery] ApprovalRequestFilterRequest request, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.GetMySubmittedAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<ApprovalRequestListItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("my-assigned")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestView)]
    public async Task<IActionResult> GetMyAssigned([FromQuery] ApprovalRequestFilterRequest request, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.GetMyAssignedAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<ApprovalRequestListItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestCreate)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] ApprovalWorkflowCommentRequest? request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = request ?? new ApprovalWorkflowCommentRequest();
        var result = await _approvalRequestService.CancelAsync(id, body, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestAssign)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] ApprovalAssignRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.AssignApproverAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/reassign")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestAssign)]
    public async Task<IActionResult> Reassign(
        Guid id,
        [FromBody] ApprovalReassignRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _approvalRequestService.ReassignApproverAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/start-review")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestReview)]
    public async Task<IActionResult> StartReview(
        Guid id,
        [FromBody] ApprovalWorkflowCommentRequest? request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = request ?? new ApprovalWorkflowCommentRequest();
        var result = await _approvalRequestService.StartReviewAsync(id, body, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestApprove)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApprovalWorkflowCommentRequest? request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = request ?? new ApprovalWorkflowCommentRequest();
        var result = await _approvalRequestService.ApproveAsync(id, body, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = PermissionCodes.ApprovalRequestReject)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ApprovalWorkflowCommentRequest? request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = request ?? new ApprovalWorkflowCommentRequest();
        var result = await _approvalRequestService.RejectAsync(id, body, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
