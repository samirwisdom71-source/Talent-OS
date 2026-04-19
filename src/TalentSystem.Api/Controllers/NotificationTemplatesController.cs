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
[Route("api/notification-templates")]
[Authorize]
public sealed class NotificationTemplatesController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationTemplatesController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.NotificationManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.CreateTemplateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<NotificationTemplateDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.NotificationManage)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.UpdateTemplateAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.NotificationView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.GetTemplateByIdAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.NotificationView)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] NotificationTemplateFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _notifications.GetTemplatesPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<NotificationTemplateDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }
}
