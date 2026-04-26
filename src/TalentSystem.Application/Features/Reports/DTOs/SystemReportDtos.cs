namespace TalentSystem.Application.Features.Reports.DTOs;

public sealed class SystemReportFilterRequest
{
    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int ChartMonths { get; set; } = 6;

    public string? Language { get; set; }
}

public sealed record SystemReportDto(
    DateTime GeneratedOnUtc,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string Language,
    int TotalTables,
    int TotalRecords,
    IReadOnlyList<SystemReportDomainSummaryDto> Domains,
    IReadOnlyList<SystemReportTableSummaryDto> Tables);

public sealed record SystemReportDomainSummaryDto(
    string DomainName,
    int TotalRecords,
    IReadOnlyList<SystemReportChartPointDto> ChartPoints,
    IReadOnlyList<SystemReportTableSummaryDto> Tables);

public sealed record SystemReportTableSummaryDto(
    string EntityName,
    string TableName,
    int RecordsCount,
    IReadOnlyList<string> PreviewColumns,
    IReadOnlyList<SystemReportTableRowDto> PreviewRows,
    IReadOnlyList<SystemReportChartPointDto> ChartPoints);

public sealed record SystemReportTableRowDto(
    IReadOnlyList<string> Cells);

public sealed record SystemReportChartPointDto(
    string Label,
    int Value);
