using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Interfaces;

public interface IIdentityLookupService
{
    Task<Result<IReadOnlyList<LookupItemDto>>> GetEmployeesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetUsersAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetRolesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetPermissionsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetPositionsAsync(
        string? search = null,
        int? take = null,
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetOrganizationUnitsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetJobGradesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetCompetenciesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetCompetencyLevelsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetPerformanceEvaluationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetTalentClassificationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetDevelopmentPlansAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetMarketplaceOpportunitiesAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetOpportunityApplicationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default);
}
