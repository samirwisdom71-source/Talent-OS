import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ExecutiveAnalyticsApiService } from '../../services/executive-analytics-api.service';
import { ExecutiveDashboardSummaryDto } from '../../shared/models/analytics.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-settings-executive-analytics-page',
  standalone: true,
  imports: [TranslatePipe, DecimalPipe, RouterLink],
  templateUrl: './settings-executive-analytics-page.component.html',
  styleUrl: './settings-executive-analytics-page.component.scss',
})
export class SettingsExecutiveAnalyticsPageComponent implements OnInit {
  private readonly analytics = inject(ExecutiveAnalyticsApiService);
  readonly summary = signal<ExecutiveDashboardSummaryDto | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.failed.set(false);
    this.analytics.getSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        this.failed.set(false);
      },
      error: () => {
        this.summary.set(null);
        this.failed.set(true);
      },
    });
  }
}
