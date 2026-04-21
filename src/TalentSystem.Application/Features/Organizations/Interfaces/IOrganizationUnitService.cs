using TalentSystem.Application.Features.Organizations.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Organizations.Interfaces;

public interface IOrganizationUnitService
{
    Task<Result<OrganizationUnitDto>> CreateAsync(
        CreateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OrganizationUnitDto>> UpdateAsync(
        Guid id,
        UpdateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OrganizationUnitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<OrganizationUnitDto>>> GetPagedAsync(
        OrganizationUnitFilterRequest request,
        CancellationToken cancellationToken = default);
}
