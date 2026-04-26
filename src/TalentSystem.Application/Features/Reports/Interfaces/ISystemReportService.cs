using TalentSystem.Application.Features.Reports.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Reports.Interfaces;

public interface ISystemReportService
{
    Task<Result<SystemReportDto>> BuildAsync(
        SystemReportFilterRequest filter,
        CancellationToken cancellationToken = default);
}
