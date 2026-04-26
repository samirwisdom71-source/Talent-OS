export interface SystemReportFilterRequest {
  fromUtc?: string | null;
  toUtc?: string | null;
  chartMonths?: number | null;
  language?: 'ar' | 'en' | null;
}

export interface SystemReportDto {
  generatedOnUtc: string;
  fromUtc?: string | null;
  toUtc?: string | null;
  language: 'ar' | 'en';
  totalTables: number;
  totalRecords: number;
  domains: SystemReportDomainSummaryDto[];
  tables: SystemReportTableSummaryDto[];
}

export interface SystemReportDomainSummaryDto {
  domainName: string;
  totalRecords: number;
  chartPoints: SystemReportChartPointDto[];
  tables: SystemReportTableSummaryDto[];
}

export interface SystemReportTableSummaryDto {
  entityName: string;
  tableName: string;
  recordsCount: number;
  previewColumns: string[];
  previewRows: SystemReportTableRowDto[];
  chartPoints: SystemReportChartPointDto[];
}

export interface SystemReportTableRowDto {
  cells: string[];
}

export interface SystemReportChartPointDto {
  label: string;
  value: number;
}
