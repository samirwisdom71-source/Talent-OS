import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild,
  effect,
  inject,
} from '@angular/core';
import type { ChartOptions } from 'chart.js';
import { DevelopmentPlanItemDto } from '../../shared/models/development-item.models';
import { DevelopmentPlanItemPathDto } from '../../shared/models/development.models';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-development-path-impact-chart',
  standalone: true,
  template: `
    <div class="path-impact-chart">
      @if (!hasPaths()) {
        <p class="path-impact-chart__empty">{{ i18n.t('development.impact.chart.emptyPaths') }}</p>
      } @else {
        <canvas #cv></canvas>
        <p class="path-impact-chart__hint">{{ i18n.t('development.impact.chart.pathCumulativeHint') }}</p>
      }
    </div>
  `,
  styles: `
    .path-impact-chart {
      position: relative;
      width: 100%;
      min-height: 260px;
      max-height: 360px;
      margin: 0;
    }
    canvas {
      max-height: 320px;
    }
    .path-impact-chart__empty {
      margin: 0;
      padding: 20px;
      text-align: center;
      color: #64748b;
      font-size: 14px;
      border: 1px dashed #e2e8f0;
      border-radius: 12px;
    }
    .path-impact-chart__hint {
      margin: 10px 0 0;
      font-size: 11px;
      color: #64748b;
      line-height: 1.45;
    }
  `,
})
export class DevelopmentPathImpactChartComponent implements AfterViewInit, OnChanges, OnDestroy {
  private static chartRegistered = false;

  @ViewChild('cv') canvasRef?: ElementRef<HTMLCanvasElement>;

  @Input() items: readonly DevelopmentPlanItemDto[] = [];

  readonly i18n = inject(I18nService);
  private readonly cdr = inject(ChangeDetectorRef);
  private chart: import('chart.js').Chart | null = null;
  private viewReady = false;

  constructor() {
    effect(() => {
      this.i18n.lang();
      if (this.viewReady) {
        void this.render();
      }
    });
  }

  hasPaths(): boolean {
    return this.flattenOrderedPaths().length > 0;
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    void this.render();
  }

  ngOnChanges(_: SimpleChanges): void {
    if (this.viewReady) {
      void this.render();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  private flattenOrderedPaths(): { path: DevelopmentPlanItemPathDto }[] {
    const out: { path: DevelopmentPlanItemPathDto }[] = [];
    for (const it of this.items) {
      const paths = [...(it.paths ?? [])].sort((a, b) => a.sortOrder - b.sortOrder);
      for (const p of paths) {
        out.push({ path: p });
      }
    }
    return out;
  }

  private buildSeries(): { labels: string[]; values: number[] } {
    const flat = this.flattenOrderedPaths();
    const labels: string[] = [this.i18n.t('development.impact.chart.pathStart')];
    const values: number[] = [0];
    let cum = 0;
    let idx = 0;
    for (const { path } of flat) {
      idx++;
      const t = path.title.trim();
      const shortTitle = t.length > 24 ? `${t.slice(0, 22)}…` : t;
      labels.push(`${idx}. ${shortTitle}`);
      if (path.status === 3) {
        cum += Number(path.achievedImpactValue ?? 0);
      }
      values.push(Math.round(cum * 10_000) / 10_000);
    }
    return { labels, values };
  }

  private layoutOptions(): ChartOptions {
    const rtl = this.i18n.isRtl();
    const locale = this.i18n.lang() === 'ar' ? 'ar' : 'en-US';
    return {
      locale,
      animation: { duration: 650, easing: 'easeOutQuart' },
      plugins: {
        tooltip: {
          rtl,
          textDirection: rtl ? 'rtl' : 'ltr',
        },
        legend: {
          rtl,
          labels: {
            textAlign: rtl ? 'right' : 'left',
          },
        },
      },
    } as ChartOptions;
  }

  private async render(): Promise<void> {
    if (!this.hasPaths()) {
      this.chart?.destroy();
      this.chart = null;
      this.cdr.detectChanges();
      return;
    }

    const el = this.canvasRef?.nativeElement;
    if (!el) return;

    const { labels, values } = this.buildSeries();

    this.chart?.destroy();
    this.chart = null;

    const mod = await import('chart.js');
    const {
      Chart,
      CategoryScale,
      Filler,
      Legend,
      LinearScale,
      LineController,
      LineElement,
      PointElement,
      Tooltip,
      Title,
    } = mod;

    if (!DevelopmentPathImpactChartComponent.chartRegistered) {
      Chart.register(
        CategoryScale,
        Legend,
        LinearScale,
        Tooltip,
        Title,
        Filler,
        LineController,
        LineElement,
        PointElement,
      );
      DevelopmentPathImpactChartComponent.chartRegistered = true;
    }

    const layout = this.layoutOptions();
    const maxY = Math.max(...values, 1);
    const pad = Math.max(2, maxY * 0.06);

    this.chart = new Chart(el, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: this.i18n.t('development.impact.chart.datasetPaths'),
            data: values,
            borderColor: '#4f46e5',
            backgroundColor: 'rgba(79, 70, 229, 0.12)',
            fill: true,
            tension: 0.25,
            stepped: 'after',
            pointRadius: 4,
            pointHoverRadius: 6,
          },
        ],
      },
      options: {
        ...layout,
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          x: {
            grid: { display: false },
            ticks: { font: { size: 10 }, maxRotation: 45, minRotation: 0 },
          },
          y: {
            beginAtZero: true,
            suggestedMax: Math.min(100, maxY + pad),
            max: 100,
            grid: { color: 'rgba(148, 163, 184, 0.2)' },
            ticks: { precision: 2 },
          },
        },
        plugins: {
          ...layout.plugins,
          legend: {
            ...layout.plugins?.legend,
            display: true,
            position: 'bottom',
            labels: {
              ...layout.plugins?.legend?.labels,
              usePointStyle: true,
              padding: 12,
              font: { size: 11 },
            },
          },
        },
      },
    });

    this.cdr.detectChanges();
  }
}
