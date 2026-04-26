using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentSystem.Application.Features.Reports.DTOs;
using TalentSystem.Application.Features.Reports.Interfaces;

namespace TalentSystem.Application.Features.Reports.Services;

public sealed class SystemReportExportService : ISystemReportExportService
{
    public byte[] BuildPdf(SystemReportDto report)
    {
        var isArabic = report.Language == "ar";
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(column =>
                {
                    column.Item().Text(isArabic ? "Talent OS - التقرير العام للنظام" : "Talent OS - System Data Report")
                        .Bold()
                        .FontSize(16)
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text(BuildFilterDescription(report))
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text(isArabic
                        ? $"تاريخ التوليد: {report.GeneratedOnUtc:yyyy-MM-dd HH:mm} UTC"
                        : $"Generated: {report.GeneratedOnUtc:yyyy-MM-dd HH:mm} UTC");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "المؤشر" : "Metric");
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "القيمة" : "Value");
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "ملاحظات" : "Notes");
                        });

                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "عدد الجداول" : "Total tables");
                        table.Cell().Element(CellBodyStyle).Text(report.TotalTables.ToString());
                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "يشمل كل جداول النظام" : "All mapped system tables");

                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "إجمالي السجلات" : "Total records");
                        table.Cell().Element(CellBodyStyle).Text(report.TotalRecords.ToString());
                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "بعد تطبيق الفلتر الزمني" : "After time filter");

                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "عدد المجموعات" : "Domain groups");
                        table.Cell().Element(CellBodyStyle).Text(report.Domains.Count.ToString());
                        table.Cell().Element(CellBodyStyle).Text(isArabic ? "تجميع حسب مجال العمل" : "Grouped by business domain");
                    });

                    column.Item().Text(isArabic ? "ملخص المجموعات" : "Domain Summary")
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "المجال" : "Domain");
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "السجلات" : "Records");
                            header.Cell().Element(CellHeaderStyle).Text(isArabic ? "الجداول" : "Tables");
                        });

                        foreach (var domain in report.Domains)
                        {
                            table.Cell().Element(CellBodyStyle).Text(domain.DomainName);
                            table.Cell().Element(CellBodyStyle).Text(domain.TotalRecords.ToString());
                            table.Cell().Element(CellBodyStyle).Text(domain.Tables.Count.ToString());
                        }
                    });

                    column.Item().Text(isArabic ? "معاينة بيانات الجداول" : "Table Data Preview")
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    foreach (var domain in report.Domains)
                    {
                        foreach (var tablePreview in domain.Tables.Where(t => t.PreviewColumns.Count > 0))
                        {
                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(section =>
                            {
                                section.Spacing(5);
                                section.Item().Text($"{tablePreview.EntityName} ({tablePreview.TableName})")
                                    .Bold()
                                    .FontColor(Colors.Blue.Medium);

                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        foreach (var _ in tablePreview.PreviewColumns)
                                        {
                                            columns.RelativeColumn();
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        foreach (var headerText in tablePreview.PreviewColumns)
                                        {
                                            header.Cell().Element(CellHeaderStyle).Text(headerText);
                                        }
                                    });

                                    foreach (var row in tablePreview.PreviewRows.Take(8))
                                    {
                                        foreach (var cell in row.Cells)
                                        {
                                            table.Cell().Element(CellBodyStyle).Text(cell);
                                        }
                                    }
                                });
                            });
                        }
                    }
                });

                page.Footer()
                    .AlignRight()
                    .Text(x =>
                    {
                        x.Span(isArabic ? "صفحة " : "Page ");
                        x.CurrentPageNumber();
                        x.Span(isArabic ? " من " : " of ");
                        x.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    public byte[] BuildExcel(SystemReportDto report)
    {
        var isArabic = report.Language == "ar";
        using var workbook = new XLWorkbook();
        var summarySheet = workbook.Worksheets.Add(isArabic ? "الملخص" : "Summary");

        summarySheet.Cell(1, 1).Value = isArabic ? "تاريخ التوليد" : "GeneratedOnUtc";
        summarySheet.Cell(1, 2).Value = report.GeneratedOnUtc;
        summarySheet.Cell(2, 1).Value = isArabic ? "من تاريخ" : "FilterFromUtc";
        summarySheet.Cell(2, 2).Value = report.FromUtc;
        summarySheet.Cell(3, 1).Value = isArabic ? "إلى تاريخ" : "FilterToUtc";
        summarySheet.Cell(3, 2).Value = report.ToUtc;
        summarySheet.Cell(4, 1).Value = isArabic ? "عدد الجداول" : "TotalTables";
        summarySheet.Cell(4, 2).Value = report.TotalTables;
        summarySheet.Cell(5, 1).Value = isArabic ? "إجمالي السجلات" : "TotalRecords";
        summarySheet.Cell(5, 2).Value = report.TotalRecords;
        summarySheet.Cell(6, 1).Value = isArabic ? "عدد المجموعات" : "Domains";
        summarySheet.Cell(6, 2).Value = report.Domains.Count;

        summarySheet.Cell(8, 1).Value = isArabic ? "المجال" : "Domain";
        summarySheet.Cell(8, 2).Value = isArabic ? "إجمالي السجلات" : "TotalRecords";
        summarySheet.Cell(8, 3).Value = isArabic ? "عدد الجداول" : "Tables";

        var domainRow = 9;
        foreach (var domain in report.Domains)
        {
            summarySheet.Cell(domainRow, 1).Value = domain.DomainName;
            summarySheet.Cell(domainRow, 2).Value = domain.TotalRecords;
            summarySheet.Cell(domainRow, 3).Value = domain.Tables.Count;
            domainRow++;
        }

        var row = domainRow + 2;
        summarySheet.Cell(row, 1).Value = isArabic ? "الكيان" : "EntityName";
        summarySheet.Cell(row, 2).Value = isArabic ? "الجدول" : "TableName";
        summarySheet.Cell(row, 3).Value = isArabic ? "السجلات" : "RecordsCount";
        row++;

        foreach (var table in report.Tables)
        {
            summarySheet.Cell(row, 1).Value = table.EntityName;
            summarySheet.Cell(row, 2).Value = table.TableName;
            summarySheet.Cell(row, 3).Value = table.RecordsCount;
            row++;
        }

        ApplyMetaSectionStyle(summarySheet, 1, 6, 2);
        ApplyHeaderStyle(summarySheet.Row(8), 3);
        if (domainRow > 9)
        {
            ApplyDataRowsStyle(summarySheet, 9, domainRow - 1, 3);
        }
        ApplyHeaderStyle(summarySheet.Row(domainRow + 2), 3);
        if (row > domainRow + 3)
        {
            ApplyDataRowsStyle(summarySheet, domainRow + 3, row - 1, 3);
            CreateStyledTable(summarySheet, domainRow + 2, row - 1, 3, "SummaryTableList");
        }
        summarySheet.SheetView.FreezeRows(8);
        summarySheet.Columns().AdjustToContents();

        foreach (var domain in report.Domains)
        {
            var sheetName = BuildWorksheetName(domain.DomainName, isArabic ? "رسم" : "Chart");
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).Value = isArabic ? "الفترة" : "Label";
            worksheet.Cell(1, 2).Value = isArabic ? "القيمة" : "Value";

            for (var i = 0; i < domain.ChartPoints.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = domain.ChartPoints[i].Label;
                worksheet.Cell(i + 2, 2).Value = domain.ChartPoints[i].Value;
            }

            ApplyHeaderStyle(worksheet.Row(1), 2);
            if (domain.ChartPoints.Count > 0)
            {
                ApplyDataRowsStyle(worksheet, 2, domain.ChartPoints.Count + 1, 2);
                CreateStyledTable(worksheet, 1, domain.ChartPoints.Count + 1, 2, $"DomainChart_{sheetName}");
            }
            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();
        }

        foreach (var domain in report.Domains)
        {
            foreach (var tablePreview in domain.Tables)
            {
                var sheetName = BuildWorksheetName(tablePreview.EntityName, tablePreview.TableName);
                var worksheet = workbook.Worksheets.Add(sheetName);

                worksheet.Cell(1, 1).Value = isArabic ? "المجال" : "Domain";
                worksheet.Cell(1, 2).Value = domain.DomainName;
                worksheet.Cell(2, 1).Value = isArabic ? "الكيان" : "Entity";
                worksheet.Cell(2, 2).Value = tablePreview.EntityName;
                worksheet.Cell(3, 1).Value = isArabic ? "الجدول" : "Table";
                worksheet.Cell(3, 2).Value = tablePreview.TableName;
                worksheet.Cell(4, 1).Value = isArabic ? "إجمالي السجلات" : "Total records";
                worksheet.Cell(4, 2).Value = tablePreview.RecordsCount;
                worksheet.Cell(5, 1).Value = isArabic ? "عدد صفوف المعاينة" : "Preview row count";
                worksheet.Cell(5, 2).Value = tablePreview.PreviewRows.Count;
                ApplyMetaSectionStyle(worksheet, 1, 5, 2);

                var headerRow = 7;
                if (tablePreview.PreviewColumns.Count > 0)
                {
                    for (var i = 0; i < tablePreview.PreviewColumns.Count; i++)
                    {
                        worksheet.Cell(headerRow, i + 1).Value = tablePreview.PreviewColumns[i];
                    }

                    var currentRow = headerRow + 1;
                    foreach (var previewDataRow in tablePreview.PreviewRows)
                    {
                        for (var i = 0; i < previewDataRow.Cells.Count; i++)
                        {
                            worksheet.Cell(currentRow, i + 1).Value = previewDataRow.Cells[i];
                        }

                        currentRow++;
                    }

                    ApplyHeaderStyle(worksheet.Row(headerRow), tablePreview.PreviewColumns.Count);
                    ApplyDataRowsStyle(worksheet, headerRow + 1, currentRow - 1, tablePreview.PreviewColumns.Count);
                    CreateStyledTable(
                        worksheet,
                        headerRow,
                        currentRow - 1,
                        tablePreview.PreviewColumns.Count,
                        $"Data_{sheetName}");
                    worksheet.SheetView.FreezeRows(headerRow);
                }
                else
                {
                    worksheet.Cell(headerRow, 1).Value = isArabic
                        ? "لا توجد أعمدة معاينة متاحة لهذا الكيان."
                        : "No previewable columns are available for this entity.";
                    worksheet.Range(headerRow, 1, headerRow, 2).Merge();
                    worksheet.Cell(headerRow, 1).Style.Font.Italic = true;
                    worksheet.Cell(headerRow, 1).Style.Font.FontColor = XLColor.DimGray;
                }

                worksheet.Columns().AdjustToContents();
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string BuildFilterDescription(SystemReportDto report)
    {
        var isArabic = report.Language == "ar";
        if (!report.FromUtc.HasValue && !report.ToUtc.HasValue)
        {
            return isArabic ? "الفلتر الزمني: كل الفترات" : "Time filter: All time";
        }

        var from = report.FromUtc?.ToString("yyyy-MM-dd") ?? "...";
        var to = report.ToUtc?.ToString("yyyy-MM-dd") ?? "...";
        return isArabic
            ? $"الفلتر الزمني: {from} -> {to}"
            : $"Time filter: {from} -> {to}";
    }

    private static string BuildWorksheetName(string entityName, string tableName)
    {
        var raw = $"{entityName}-{tableName}";
        var sanitized = new string(raw
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_')
            .ToArray());

        if (sanitized.Length == 0)
        {
            sanitized = "Table";
        }

        return sanitized.Length <= 31 ? sanitized : sanitized[..31];
    }

    private static IContainer CellHeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Blue.Lighten4)
            .Border(1)
            .BorderColor(Colors.Blue.Lighten2)
            .Padding(6)
            .DefaultTextStyle(style => style.SemiBold().FontSize(9));
    }

    private static IContainer CellBodyStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(6)
            .DefaultTextStyle(style => style.FontSize(9));
    }

    private static void ApplyMetaSectionStyle(IXLWorksheet worksheet, int startRow, int endRow, int columnCount)
    {
        for (var row = startRow; row <= endRow; row++)
        {
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0ECFF");
            worksheet.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Cell(row, 1).Style.Border.OutsideBorderColor = XLColor.FromHtml("#A8C5F5");

            for (var col = 2; col <= columnCount; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml("#D2DCEB");
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            }
        }
    }

    private static void ApplyHeaderStyle(IXLRow row, int columnCount)
    {
        for (var col = 1; col <= columnCount; col++)
        {
            var cell = row.Cell(col);
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D4ED8");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#B7C6DF");
        }
    }

    private static void ApplyDataRowsStyle(IXLWorksheet worksheet, int startRow, int endRow, int columnCount)
    {
        if (endRow < startRow)
        {
            return;
        }

        for (var row = startRow; row <= endRow; row++)
        {
            for (var col = 1; col <= columnCount; col++)
            {
                var cell = worksheet.Cell(row, col);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D2DCEB");
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }

    private static void CreateStyledTable(IXLWorksheet worksheet, int firstRow, int lastRow, int columnCount, string baseName)
    {
        if (lastRow <= firstRow || columnCount <= 0)
        {
            return;
        }

        var range = worksheet.Range(firstRow, 1, lastRow, columnCount);
        var safeName = new string(baseName.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "Table";
        }
        if (safeName.Length > 40)
        {
            safeName = safeName[..40];
        }

        var table = range.CreateTable(safeName);
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowAutoFilter = true;
    }
}
