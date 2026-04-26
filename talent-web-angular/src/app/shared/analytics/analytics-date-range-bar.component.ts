import { Component, input, output, signal } from '@angular/core';
import { TranslatePipe } from '../pipes/translate.pipe';

export interface AnalyticsDateRangePayload {
  readonly fromUtc: string;
  readonly toUtc: string;
}

export type AnalyticsDateRangeVariant = 'executive' | 'kpis';

@Component({
  selector: 'app-analytics-date-range-bar',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './analytics-date-range-bar.component.html',
  styleUrl: './analytics-date-range-bar.component.scss',
})
export class AnalyticsDateRangeBarComponent {
  /** Visual style to match the host analytics page. */
  readonly variant = input<AnalyticsDateRangeVariant>('kpis');

  /** Fired with ISO UTC bounds, or `null` when cleared. */
  readonly rangeChange = output<AnalyticsDateRangePayload | null>();

  readonly draftFrom = signal('');
  readonly draftTo = signal('');
  readonly invalidOrder = signal(false);

  hintKey(): string {
    return this.variant() === 'executive'
      ? 'analytics.dateRange.hintExecutive'
      : 'analytics.dateRange.hintPerformanceKpis';
  }

  setFrom(ev: Event): void {
    const v = (ev.target as HTMLInputElement).value;
    this.draftFrom.set(v);
    this.invalidOrder.set(false);
  }

  setTo(ev: Event): void {
    const v = (ev.target as HTMLInputElement).value;
    this.draftTo.set(v);
    this.invalidOrder.set(false);
  }

  apply(): void {
    const f = this.draftFrom();
    const t = this.draftTo();
    if (!f || !t) {
      return;
    }
    if (f > t) {
      this.invalidOrder.set(true);
      return;
    }
    this.invalidOrder.set(false);
    this.rangeChange.emit({
      fromUtc: new Date(`${f}T00:00:00.000Z`).toISOString(),
      toUtc: new Date(`${t}T23:59:59.999Z`).toISOString(),
    });
  }

  clear(): void {
    this.draftFrom.set('');
    this.draftTo.set('');
    this.invalidOrder.set(false);
    this.rangeChange.emit(null);
  }
}
