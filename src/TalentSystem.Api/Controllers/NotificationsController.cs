using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Identity;
using TalentSystem.Application.Features.Notifications.DTOs;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.NotificationManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<NotificationDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("batch")]
    [Authorize(Policy = PermissionCodes.NotificationManage)]
    public async Task<IActionResult> CreateBatch(
        [FromBody] CreateNotificationBatchRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.CreateBatchAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.GetByIdAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(
        [FromQuery] NotificationFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.GetMyNotificationsAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<NotificationListItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("my/unread-count")]
    public async Task<IActionResult> GetMyUnreadCount(CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.GetMyUnreadCountAsync(cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.MarkReadAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("my/mark-all-read")]
    public async Task<IActionResult> MarkAllMyRead(CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.MarkAllMyReadAsync(cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
