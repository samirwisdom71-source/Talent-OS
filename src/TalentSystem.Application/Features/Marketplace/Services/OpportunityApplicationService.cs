using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Application.Features.Notifications.Integration;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Marketplace.Services;

public sealed class OpportunityApplicationService : IOpportunityApplicationService
{
    private static readonly OpportunityApplicationStatus[] ApplicantCapStatuses =
    {
        OpportunityApplicationStatus.Submitted,
        OpportunityApplicationStatus.UnderReview,
        OpportunityApplicationStatus.Shortlisted,
        OpportunityApplicationStatus.Accepted
    };

    private readonly TalentDbContext _db;
    private readonly IValidator<ApplyOpportunityRequest> _applyValidator;
    private readonly IValidator<UpdateOpportunityApplicationRequest> _updateValidator;
    private readonly IValidator<OpportunityApplicationFilterRequest> _filterValidator;
    private readonly INotificationService _notifications;

    public OpportunityApplicationService(
        TalentDbContext db,
        IValidator<ApplyOpportunityRequest> applyValidator,
        IValidator<UpdateOpportunityApplicationRequest> updateValidator,
        IValidator<OpportunityApplicationFilterRequest> filterValidator,
        INotificationService notifications)
    {
        _db = db;
        _applyValidator = applyValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _notifications = notifications;
    }

    public async Task<Result<OpportunityApplicationDto>> ApplyAsync(
        ApplyOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _applyValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OpportunityApplicationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var opportunity = await _db.MarketplaceOpportunities.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.MarketplaceOpportunityId, cancellationToken);

        if (opportunity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        if (!IsOpportunityAcceptingNewApplications(opportunity, DateTime.UtcNow))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Applications are only accepted while the opportunity is open and within its published dates.",
                MarketplaceErrors.OpportunityNotOpen);
        }

        var employee = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        if (opportunity.OpportunityType == OpportunityType.InternalRole &&
            opportunity.PositionId is { } targetRoleId &&
            employee.PositionId == targetRoleId)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Employees already holding the target internal role cannot apply to this opportunity.",
                MarketplaceErrors.ApplicantAlreadyInTargetRole);
        }

        if (await _db.OpportunityApplications.AsNoTracking().AnyAsync(
                x => x.MarketplaceOpportunityId == request.MarketplaceOpportunityId &&
                     x.EmployeeId == request.EmployeeId,
                cancellationToken))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "This employee has already applied to this opportunity.",
                MarketplaceErrors.ApplicationDuplicate);
        }

        if (opportunity.MaxApplicants is { } max)
        {
            var current = await CountPipelineApplicationsAsync(request.MarketplaceOpportunityId, cancellationToken);
            if (current >= max)
            {
                return Result<OpportunityApplicationDto>.Fail(
                    "The maximum number of applicants for this opportunity has been reached.",
                    MarketplaceErrors.MaxApplicantsReached);
            }
        }

        var entity = new OpportunityApplication
        {
            MarketplaceOpportunityId = request.MarketplaceOpportunityId,
            EmployeeId = request.EmployeeId,
            ApplicationStatus = OpportunityApplicationStatus.Submitted,
            MotivationStatement = string.IsNullOrWhiteSpace(request.MotivationStatement)
                ? null
                : request.MotivationStatement.Trim(),
            AppliedOnUtc = DateTime.UtcNow,
            ReviewedOnUtc = null
        };

        _db.OpportunityApplications.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await OpportunityMatchSnapshotRefresher.UpsertAsync(
            _db,
            request.MarketplaceOpportunityId,
            request.EmployeeId,
            opportunity,
            employee,
            cancellationToken);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> UpdateAsync(
        Guid id,
        UpdateOpportunityApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OpportunityApplicationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (!ApplicationAllowsCandidateEdits(entity))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "This application cannot be updated in its current status.",
                MarketplaceErrors.ApplicationReadOnly);
        }

        if (entity.MarketplaceOpportunity.Status != MarketplaceOpportunityStatus.Open)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Applications can only be updated while the opportunity is open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        entity.MotivationStatement = string.IsNullOrWhiteSpace(request.MotivationStatement)
            ? null
            : request.MotivationStatement.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await (
            from x in _db.OpportunityApplications.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on x.EmployeeId equals e.Id
            join o in _db.MarketplaceOpportunities.AsNoTracking() on x.MarketplaceOpportunityId equals o.Id
            where x.Id == id
            select new OpportunityApplicationDto
            {
                Id = x.Id,
                MarketplaceOpportunityId = x.MarketplaceOpportunityId,
                EmployeeId = x.EmployeeId,
                EmployeeDisplayName = string.IsNullOrWhiteSpace(e.FullNameAr) ? e.FullNameEn : e.FullNameAr,
                MarketplaceOpportunityTitle = o.Title,
                ApplicationStatus = x.ApplicationStatus,
                MotivationStatement = x.MotivationStatement,
                AppliedOnUtc = x.AppliedOnUtc,
                ReviewedOnUtc = x.ReviewedOnUtc,
                Notes = x.Notes
            }).FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        return Result<OpportunityApplicationDto>.Ok(dto);
    }

    public async Task<Result<PagedResult<OpportunityApplicationDto>>> GetPagedAsync(
        OpportunityApplicationFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<OpportunityApplicationDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<OpportunityApplication> query = _db.OpportunityApplications.AsNoTracking();

        if (request.MarketplaceOpportunityId is { } oppId && oppId != Guid.Empty)
        {
            query = query.Where(x => x.MarketplaceOpportunityId == oppId);
        }

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var baseQuery =
            from x in query
            join e in _db.Employees.AsNoTracking() on x.EmployeeId equals e.Id
            join o in _db.MarketplaceOpportunities.AsNoTracking() on x.MarketplaceOpportunityId equals o.Id
            select new { x, e, o };

        var items = await baseQuery
            .OrderByDescending(t => t.x.AppliedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new OpportunityApplicationDto
            {
                Id = t.x.Id,
                MarketplaceOpportunityId = t.x.MarketplaceOpportunityId,
                EmployeeId = t.x.EmployeeId,
                EmployeeDisplayName = string.IsNullOrWhiteSpace(t.e.FullNameAr) ? t.e.FullNameEn : t.e.FullNameAr,
                MarketplaceOpportunityTitle = t.o.Title,
                ApplicationStatus = t.x.ApplicationStatus,
                MotivationStatement = t.x.MotivationStatement,
                AppliedOnUtc = t.x.AppliedOnUtc,
                ReviewedOnUtc = t.x.ReviewedOnUtc,
                Notes = t.x.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OpportunityApplicationDto>>.Ok(new PagedResult<OpportunityApplicationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<OpportunityApplicationDto>> WithdrawAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (entity.MarketplaceOpportunity.Status != MarketplaceOpportunityStatus.Open)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Applications cannot be withdrawn while the opportunity is not open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (entity.ApplicationStatus == OpportunityApplicationStatus.Withdrawn)
        {
            return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
        }

        if (entity.ApplicationStatus is not (
            OpportunityApplicationStatus.Submitted
            or OpportunityApplicationStatus.UnderReview
            or OpportunityApplicationStatus.Shortlisted))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "This application cannot be withdrawn from its current status.",
                MarketplaceErrors.InvalidApplicationStatusTransition);
        }

        entity.ApplicationStatus = OpportunityApplicationStatus.Withdrawn;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> MarkUnderReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (!EnsureOpportunityOpenForWorkflow(entity))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Recruiter actions require the opportunity to be open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (entity.ApplicationStatus != OpportunityApplicationStatus.Submitted)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Only submitted applications can be marked under review.",
                MarketplaceErrors.InvalidApplicationStatusTransition);
        }

        entity.ApplicationStatus = OpportunityApplicationStatus.UnderReview;
        TouchReviewedOnUtc(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> ShortlistAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (!EnsureOpportunityOpenForWorkflow(entity))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Recruiter actions require the opportunity to be open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (entity.ApplicationStatus is not (
            OpportunityApplicationStatus.Submitted
            or OpportunityApplicationStatus.UnderReview))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Only submitted or under-review applications can be shortlisted.",
                MarketplaceErrors.InvalidApplicationStatusTransition);
        }

        entity.ApplicationStatus = OpportunityApplicationStatus.Shortlisted;
        TouchReviewedOnUtc(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> AcceptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (!EnsureOpportunityOpenForWorkflow(entity))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Recruiter actions require the opportunity to be open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (entity.ApplicationStatus != OpportunityApplicationStatus.Shortlisted)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Only shortlisted applications can be accepted.",
                MarketplaceErrors.InvalidApplicationStatusTransition);
        }

        entity.ApplicationStatus = OpportunityApplicationStatus.Accepted;
        TouchReviewedOnUtc(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await NotifyApplicantAsync(
                entity.Id,
                entity.EmployeeId,
                entity.MarketplaceOpportunity.Title,
                accept: true,
                cancellationToken)
            .ConfigureAwait(false);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<OpportunityApplicationDto>> RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.OpportunityApplications
            .Include(x => x.MarketplaceOpportunity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<OpportunityApplicationDto>.Fail(
                "The opportunity application was not found.",
                MarketplaceErrors.ApplicationNotFound);
        }

        if (!EnsureOpportunityOpenForWorkflow(entity))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "Recruiter actions require the opportunity to be open.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (entity.ApplicationStatus is not (
            OpportunityApplicationStatus.Submitted
            or OpportunityApplicationStatus.UnderReview
            or OpportunityApplicationStatus.Shortlisted))
        {
            return Result<OpportunityApplicationDto>.Fail(
                "This application cannot be rejected from its current status.",
                MarketplaceErrors.InvalidApplicationStatusTransition);
        }

        entity.ApplicationStatus = OpportunityApplicationStatus.Rejected;
        TouchReviewedOnUtc(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await NotifyApplicantAsync(
                entity.Id,
                entity.EmployeeId,
                entity.MarketplaceOpportunity.Title,
                accept: false,
                cancellationToken)
            .ConfigureAwait(false);

        return Result<OpportunityApplicationDto>.Ok(await MapToDtoTrackedAsync(entity.Id, cancellationToken));
    }

    private static bool EnsureOpportunityOpenForWorkflow(OpportunityApplication entity) =>
        entity.MarketplaceOpportunity.Status == MarketplaceOpportunityStatus.Open;

    private static bool IsOpportunityAcceptingNewApplications(
        MarketplaceOpportunity opportunity,
        DateTime utcNow)
    {
        if (opportunity.Status != MarketplaceOpportunityStatus.Open)
        {
            return false;
        }

        if (utcNow.Date < opportunity.OpenDate.Date)
        {
            return false;
        }

        if (opportunity.CloseDate is { } close && utcNow.Date > close.Date)
        {
            return false;
        }

        return true;
    }

    private async Task<int> CountPipelineApplicationsAsync(
        Guid marketplaceOpportunityId,
        CancellationToken cancellationToken) =>
        await _db.OpportunityApplications.CountAsync(
            x => x.MarketplaceOpportunityId == marketplaceOpportunityId &&
                 ApplicantCapStatuses.Contains(x.ApplicationStatus),
            cancellationToken);

    private static bool ApplicationAllowsCandidateEdits(OpportunityApplication entity) =>
        entity.ApplicationStatus is
            OpportunityApplicationStatus.Submitted
            or OpportunityApplicationStatus.UnderReview
            or OpportunityApplicationStatus.Shortlisted;

    private static void TouchReviewedOnUtc(OpportunityApplication entity)
    {
        if (entity.ApplicationStatus is OpportunityApplicationStatus.UnderReview
            or OpportunityApplicationStatus.Shortlisted
            or OpportunityApplicationStatus.Accepted
            or OpportunityApplicationStatus.Rejected)
        {
            entity.ReviewedOnUtc ??= DateTime.UtcNow;
        }
    }

    private async Task NotifyApplicantAsync(
        Guid applicationId,
        Guid employeeId,
        string opportunityTitle,
        bool accept,
        CancellationToken cancellationToken)
    {
        var userId = await _db.Users.AsNoTracking()
            .Where(u => u.EmployeeId == employeeId && u.IsActive)
            .OrderBy(u => u.CreatedOnUtc)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (userId is null)
        {
            return;
        }

        var req = accept
            ? MarketplaceNotificationRequests.ForApplicationAccepted(applicationId, userId.Value, opportunityTitle)
            : MarketplaceNotificationRequests.ForApplicationRejected(applicationId, userId.Value, opportunityTitle);

        await _notifications.PublishIntegrationAsync(req, cancellationToken).ConfigureAwait(false);
    }

    private async Task<OpportunityApplicationDto> MapToDtoTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        return await (
            from x in _db.OpportunityApplications.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on x.EmployeeId equals e.Id
            join o in _db.MarketplaceOpportunities.AsNoTracking() on x.MarketplaceOpportunityId equals o.Id
            where x.Id == id
            select new OpportunityApplicationDto
            {
                Id = x.Id,
                MarketplaceOpportunityId = x.MarketplaceOpportunityId,
                EmployeeId = x.EmployeeId,
                EmployeeDisplayName = string.IsNullOrWhiteSpace(e.FullNameAr) ? e.FullNameEn : e.FullNameAr,
                MarketplaceOpportunityTitle = o.Title,
                ApplicationStatus = x.ApplicationStatus,
                MotivationStatement = x.MotivationStatement,
                AppliedOnUtc = x.AppliedOnUtc,
                ReviewedOnUtc = x.ReviewedOnUtc,
                Notes = x.Notes
            }).FirstAsync(cancellationToken);
    }
}
