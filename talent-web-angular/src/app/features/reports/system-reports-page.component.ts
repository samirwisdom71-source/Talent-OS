import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { ChartWidgetComponent } from '../../shared/charts/chart-widget.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { SystemReportsApiService } from '../../services/system-reports-api.service';
import { SystemReportDto, SystemReportFilterRequest } from '../../shared/models/system-report.models';
import { I18nService } from '../../shared/services/i18n.service';
import { ChartWidgetKind } from '../../shared/charts/chart-widget.component';

@Component({
  selector: 'app-system-reports-page',
  standalone: true,
  imports: [FormsModule, DatePipe, TranslatePipe, ChartWidgetComponent, EmptyStateComponent],
  templateUrl: './system-reports-page.component.html',
  styleUrl: './system-reports-page.component.scss',
})
export class SystemReportsPageComponent implements OnInit {
  private readonly reportsApi = inject(SystemReportsApiService);
  private readonly i18n = inject(I18nService);

  readonly report = signal<SystemReportDto | null>(null);
  readonly loading = signal(false);
  readonly failed = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingExcel = signal(false);

  readonly filter = signal<SystemReportFilterRequest>({
    fromUtc: null,
    toUtc: null,
    chartMonths: 6,
    language: 'ar',
  });

  readonly chartCards = computed(() => {
    const current = this.report();
    if (!current) return [];
    const kinds: ChartWidgetKind[] = ['line', 'bar', 'pie'];

    return current.domains
      .filter((domain) => domain.chartPoints.length > 0)
      .sort((a, b) => b.totalRecords - a.totalRecords)
      .map((domain, index) => ({
        title: domain.domainName,
        subtitle:
          current.language === 'ar'
            ? `${domain.tables.length} جداول`
            : `${domain.tables.length} tables`,
        total: domain.totalRecords,
        labels: domain.chartPoints.map((point) => point.label),
        values: domain.chartPoints.map((point) => point.value),
        kind: kinds[index % kinds.length],
      }));
  });

  ngOnInit(): void {
    this.syncLanguage();
    this.applyFilter();
  }

  applyFilter(): void {
    this.syncLanguage();
    this.loading.set(true);
    this.failed.set(false);

    this.reportsApi.getReport(this.filter()).subscribe({
      next: (data) => {
        this.report.set(data);
        this.loading.set(false);
        this.failed.set(false);
      },
      error: () => {
        this.report.set(null);
        this.loading.set(false);
        this.failed.set(true);
      },
    });
  }

  clearFilter(): void {
    this.filter.set({
      fromUtc: null,
      toUtc: null,
      chartMonths: 6,
      language: this.i18n.lang(),
    });
    this.applyFilter();
  }

  updateFromDate(value: string | null): void {
    const current = this.filter();
    this.filter.set({
      ...current,
      fromUtc: value || null,
    });
  }

  updateToDate(value: string | null): void {
    const current = this.filter();
    this.filter.set({
      ...current,
      toUtc: value || null,
    });
  }

  updateChartMonths(value: number | string | null): void {
    const parsed = Number(value);
    const months = Number.isFinite(parsed) ? Math.min(24, Math.max(1, parsed)) : 6;
    const current = this.filter();
    this.filter.set({
      ...current,
      chartMonths: months,
    });
  }

  exportPdf(): void {
    this.syncLanguage();
    this.exportingPdf.set(true);
    this.reportsApi.exportPdf(this.filter()).subscribe({
      next: (blob) => {
        this.exportingPdf.set(false);
        this.downloadBlob(blob, 'system-report.pdf');
      },
      error: () => {
        this.exportingPdf.set(false);
      },
    });
  }

  exportExcel(): void {
    this.syncLanguage();
    this.exportingExcel.set(true);
    this.reportsApi.exportExcel(this.filter()).subscribe({
      next: (blob) => {
        this.exportingExcel.set(false);
        this.downloadBlob(blob, 'system-report.xlsx');
      },
      error: () => {
        this.exportingExcel.set(false);
      },
    });
  }

  private downloadBlob(blob: Blob, fallbackName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fallbackName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  private syncLanguage(): void {
    const current = this.filter();
    this.filter.set({
      ...current,
      language: this.i18n.lang(),
    });
  }
}
