using Microsoft.AspNetCore.Mvc;
using TalentSystem.Application.Common;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Api.Extensions;

public static class ResultHttpExtensions
{
    private static readonly HashSet<string?> NotFoundCodes = new(StringComparer.Ordinal)
    {
        EmployeeErrors.NotFound,
        EmployeeErrors.OrganizationUnitNotFound,
        EmployeeErrors.PositionNotFound,
        CompetencyErrors.CategoryNotFound,
        CompetencyErrors.CompetencyNotFound,
        CompetencyErrors.LevelNotFound,
        CompetencyErrors.CompetencyCategoryMissing,
        CompetencyErrors.JobRequirementNotFound,
        CompetencyErrors.JobRequirementPositionNotFound,
        CompetencyErrors.JobRequirementCompetencyNotFound,
        CompetencyErrors.JobRequirementRequiredLevelNotFound,
        PerformanceErrors.CycleNotFound,
        PerformanceErrors.GoalNotFound,
        PerformanceErrors.EvaluationNotFound,
        PerformanceErrors.EmployeeNotFound,
        PotentialErrors.AssessmentNotFound,
        PotentialErrors.AssessorNotFound,
        ScoringErrors.TalentScoreNotFound,
        ScoringErrors.PolicyNotFound,
        ClassificationErrors.ClassificationNotFound,
        ClassificationErrors.RuleSetNotFound,
        SuccessionErrors.CriticalPositionNotFound,
        SuccessionErrors.SuccessionPlanNotFound,
        SuccessionErrors.SuccessorCandidateNotFound,
        DevelopmentErrors.DevelopmentPlanNotFound,
        DevelopmentErrors.DevelopmentPlanItemNotFound,
        DevelopmentErrors.DevelopmentPlanItemPathNotFound,
        MarketplaceErrors.OpportunityNotFound,
        MarketplaceErrors.ApplicationNotFound,
        IdentityErrors.UserNotFound,
        IdentityErrors.RoleNotFound,
        IdentityErrors.PermissionNotFound,
        IdentityErrors.EmployeeNotFound,
        ApprovalErrors.NotFound,
        ApprovalErrors.ApproverNotFound,
        ApprovalErrors.RequestedUserNotFound,
        NotificationErrors.NotFound,
        NotificationErrors.TemplateNotFound,
        IntelligenceErrors.InsightNotFound,
        IntelligenceErrors.RecommendationNotFound
    };

    private static readonly HashSet<string?> ConflictCodes = new(StringComparer.Ordinal)
    {
        EmployeeErrors.DuplicateEmployeeNumber,
        EmployeeErrors.DuplicateEmail,
        CompetencyErrors.DuplicateCompetencyCode,
        CompetencyErrors.DuplicateCompetencyLevelNumericValue,
        CompetencyErrors.DuplicateJobRequirement,
        PerformanceErrors.EvaluationDuplicate,
        PerformanceErrors.GoalReadOnly,
        PerformanceErrors.EvaluationReadOnly,
        PerformanceErrors.GoalWeightExceeded,
        PotentialErrors.AssessmentDuplicate,
        PotentialErrors.AssessmentReadOnly,
        ScoringErrors.TalentScoreAlreadyExists,
        ScoringErrors.PolicyVersionDuplicate,
        ClassificationErrors.ClassificationAlreadyExists,
        ClassificationErrors.RuleSetVersionDuplicate,
        SuccessionErrors.CriticalPositionDuplicate,
        SuccessionErrors.SuccessionPlanDuplicate,
        SuccessionErrors.SuccessorCandidateDuplicate,
        SuccessionErrors.SuccessorCandidateRankDuplicate,
        DevelopmentErrors.DevelopmentPlanLinkDuplicate,
        MarketplaceErrors.ApplicationDuplicate,
        MarketplaceErrors.MaxApplicantsReached,
        IdentityErrors.DuplicateUserName,
        IdentityErrors.DuplicateEmail,
        IdentityErrors.DuplicateRoleName,
        IdentityErrors.EmployeeAlreadyLinked,
        NotificationErrors.DuplicateTemplateCode
    };

    private static readonly HashSet<string?> BadRequestCodes = new(StringComparer.Ordinal)
    {
        PerformanceErrors.CycleMustBeDraftToActivate,
        PerformanceErrors.CycleMustBeActiveToClose,
        PerformanceErrors.CycleReadOnly,
        PerformanceErrors.GoalCycleNotOpen,
        PerformanceErrors.EvaluationCycleClosed,
        PerformanceErrors.CycleInvalidDateRange,
        PotentialErrors.AssessmentCycleClosed,
        ScoringErrors.CycleArchivedCannotScore,
        ScoringErrors.MissingPerformanceEvaluation,
        ScoringErrors.MissingPotentialAssessment,
        ScoringErrors.SourceNotFinalized,
        ScoringErrors.PolicyMustBeInactiveToActivate,
        ClassificationErrors.CycleArchivedCannotClassify,
        ClassificationErrors.TalentScoreRequired,
        ClassificationErrors.RuleSetMustBeInactiveToActivate,
        SuccessionErrors.CriticalPositionInactive,
        SuccessionErrors.CriticalPositionReadOnly,
        SuccessionErrors.SuccessionPlanReadOnly,
        SuccessionErrors.CycleArchivedCannotCreatePlan,
        SuccessionErrors.InvalidPlanStatusTransition,
        SuccessionErrors.CandidateOccupiesTargetPosition,
        DevelopmentErrors.DevelopmentPlanReadOnly,
        DevelopmentErrors.CycleArchivedCannotCreatePlan,
        DevelopmentErrors.InvalidDevelopmentPlanStatusTransition,
        DevelopmentErrors.DevelopmentPlanItemReadOnly,
        DevelopmentErrors.InvalidItemStatusForOperation,
        DevelopmentErrors.ItemPathsIncomplete,
        MarketplaceErrors.OpportunityReadOnly,
        MarketplaceErrors.OpportunityNotOpen,
        MarketplaceErrors.ApplicationReadOnly,
        MarketplaceErrors.InvalidOpportunityStatusTransition,
        MarketplaceErrors.InvalidApplicationStatusTransition,
        MarketplaceErrors.ApplicantAlreadyInTargetRole,
        IdentityErrors.SystemRoleNameImmutable,
        IdentityErrors.SystemRoleCannotDelete,
        ApprovalErrors.DraftOnly,
        ApprovalErrors.InvalidStatus,
        ApprovalErrors.InvalidStateForAssign,
        ApprovalErrors.NotRequester,
        ApprovalErrors.NotCurrentApprover,
        ApprovalErrors.ApproverInactive,
        NotificationErrors.NoValidUsersInBatch,
        NotificationErrors.NotOwner,
        NotificationErrors.UserNotFound,
        NotificationErrors.UserInactive,
        IntelligenceErrors.CycleArchived,
        IntelligenceErrors.InvalidInsightStatus,
        IntelligenceErrors.InvalidRecommendationStatus
    };

    private static readonly HashSet<string?> UnauthorizedCodes = new(StringComparer.Ordinal)
    {
        IdentityErrors.InvalidCredentials,
        IdentityErrors.UserInactive,
        ApprovalErrors.CurrentUserRequired,
        NotificationErrors.CurrentUserRequired
    };

    public static IActionResult ToEmployeeActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string? traceId) =>
        result.ToApiActionResult(controller, traceId);

    public static IActionResult ToApiActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string? traceId)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(ApiResponse<T>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(controller, traceId);
    }

    public static IActionResult ToFailureActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string? traceId)
    {
        var errors = result.Errors;
        var payload = ApiResponse<T>.FromFailure(errors, traceId);

        if (NotFoundCodes.Contains(result.FailureCode))
        {
            return controller.NotFound(payload);
        }

        if (ConflictCodes.Contains(result.FailureCode))
        {
            return controller.Conflict(payload);
        }

        if (BadRequestCodes.Contains(result.FailureCode))
        {
            return controller.BadRequest(payload);
        }

        if (UnauthorizedCodes.Contains(result.FailureCode))
        {
            return controller.Unauthorized(payload);
        }

        if (result.FailureCode == EmployeeErrors.PositionOrganizationMismatch)
        {
            return controller.BadRequest(payload);
        }

        return controller.BadRequest(payload);
    }

    public static IActionResult ToApiActionResult(
        this Result result,
        ControllerBase controller,
        string? traceId)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(ApiResponse.FromSuccess(traceId));
        }

        return result.ToFailureActionResult(controller, traceId);
    }

    public static IActionResult ToFailureActionResult(
        this Result result,
        ControllerBase controller,
        string? traceId)
    {
        var payload = ApiResponse.FromFailure(result.Errors, traceId);

        if (NotFoundCodes.Contains(result.FailureCode))
        {
            return controller.NotFound(payload);
        }

        if (ConflictCodes.Contains(result.FailureCode))
        {
            return controller.Conflict(payload);
        }

        if (BadRequestCodes.Contains(result.FailureCode))
        {
            return controller.BadRequest(payload);
        }

        if (UnauthorizedCodes.Contains(result.FailureCode))
        {
            return controller.Unauthorized(payload);
        }

        return controller.BadRequest(payload);
    }
}
