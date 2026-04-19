import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ExecutiveAnalyticsApiService } from '../../services/executive-analytics-api.service';
import { ExecutiveDashboardSummaryDto } from '../../shared/models/analytics.models';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';

@Component({
  selector: 'app-executive-analytics-page',
  standalone: true,
  imports: [DecimalPipe, RouterLink, EmptyStateComponent],
  templateUrl: './executive-analytics-page.component.html',
  styleUrl: './executive-analytics-page.component.scss',
})
export class ExecutiveAnalyticsPageComponent implements OnInit {
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
