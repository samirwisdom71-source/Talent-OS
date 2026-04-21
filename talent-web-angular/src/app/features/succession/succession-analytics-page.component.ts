import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { SuccessionAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-succession-analytics-page',
  standalone: true,
  imports: [TranslatePipe, DecimalPipe],
  templateUrl: './succession-analytics-page.component.html',
  styleUrl: './succession-analytics-page.component.scss',
})
export class SuccessionAnalyticsPageComponent implements OnInit {
  private readonly api = inject(DomainAnalyticsApiService);
  readonly i18n = inject(I18nService);

  readonly summary = signal<SuccessionAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);

  ngOnInit(): void {
    this.busy.set(true);
    this.api.getSuccessionSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        this.busy.set(false);
        this.failed.set(false);
      },
      error: () => {
        this.summary.set(null);
        this.busy.set(false);
        this.failed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  readinessLabel(row: { value: number; count: number }): string {
    return EnumLabels.readinessLevel(this.lang(), row.value);
  }
}
