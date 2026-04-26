using TalentSystem.Application.Features.Reports.DTOs;

namespace TalentSystem.Application.Features.Reports.Interfaces;

public interface ISystemReportExportService
{
    byte[] BuildPdf(SystemReportDto report);

    byte[] BuildExcel(SystemReportDto report);
}
