import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartWidgetComponent } from '../../shared/charts/chart-widget.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { PerformanceImpactFilterRequest, PerformanceImpactSummaryDto } from '../../shared/models/domain-analytics.models';
import { I18nService } from '../../shared/services/i18n.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';

@Component({
  selector: 'app-impact-measurement-page',
  standalone: true,
  imports: [FormsModule, DecimalPipe, TranslatePipe, ChartWidgetComponent, EmptyStateComponent],
  templateUrl: './impact-measurement-page.component.html',
  styleUrl: './impact-measurement-page.component.scss',
})
export class ImpactMeasurementPageComponent implements OnInit {
  private readonly api = inject(DomainAnalyticsApiService);
  private readonly i18n = inject(I18nService);

  readonly loading = signal(false);
  readonly failed = signal(false);
  readonly impact = signal<PerformanceImpactSummaryDto | null>(null);
  readonly filter = signal<PerformanceImpactFilterRequest>({});

  /** Chart category labels — neutral wording (no «before/after» in UI). */
  readonly impactChartLabels = computed(() => {
    this.i18n.lang();
    return [this.i18n.t('analytics.impact.period1'), this.i18n.t('analytics.impact.period2')];
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.api.getPerformanceImpact(this.filter()).subscribe({
      next: (impact) => {
        this.impact.set(impact);
        this.loading.set(false);
      },
      error: () => {
        this.impact.set(null);
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  updateFilter(key: keyof PerformanceImpactFilterRequest, value: string): void {
    this.filter.set({ ...this.filter(), [key]: value || null });
  }
}
