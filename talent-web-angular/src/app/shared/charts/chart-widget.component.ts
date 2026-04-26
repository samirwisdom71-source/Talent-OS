import {
  AfterViewInit,
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
import { I18nService } from '../services/i18n.service';

export type ChartWidgetKind = 'doughnut' | 'bar' | 'line' | 'pie';

@Component({
  selector: 'app-chart-widget',
  standalone: true,
  template: `<div class="chart-widget"><canvas #cv></canvas></div>`,
  styles: `
    .chart-widget {
      position: relative;
      width: 100%;
      min-height: 220px;
      max-height: 280px;
    }
    canvas {
      max-height: 260px;
    }
  `,
})
export class ChartWidgetComponent implements AfterViewInit, OnChanges, OnDestroy {
  private static chartRegistered = false;

  private readonly i18n = inject(I18nService);

  @ViewChild('cv') canvasRef?: ElementRef<HTMLCanvasElement>;

  @Input({ required: true }) kind!: ChartWidgetKind;
  @Input({ required: true }) labels: string[] = [];
  @Input({ required: true }) values: number[] = [];
  @Input() colors?: string[];
  /** Chart.js dataset label (legend) */
  @Input() datasetLabel = '';

  private chart: import('chart.js').Chart | null = null;
  private viewReady = false;

  constructor() {
    effect(() => {
      this.i18n.lang();
      if (!this.viewReady || !this.canvasRef?.nativeElement) return;
      if (!this.labels?.length || !this.values?.length) return;
      void this.render();
    });
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    void this.render();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.viewReady || !this.canvasRef?.nativeElement) return;
    if (changes['labels'] || changes['values'] || changes['colors'] || changes['kind'] || changes['datasetLabel']) {
      void this.render();
    }
  }

  ngOnDestroy(): void {
    this.destroy();
  }

  private destroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  private layoutOptions(): ChartOptions {
    const rtl = this.i18n.isRtl();
    const locale = this.i18n.lang() === 'ar' ? 'ar' : 'en-US';
    return {
      locale,
      animation: { duration: 900, easing: 'easeOutQuart' },
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
    const el = this.canvasRef?.nativeElement;
    if (!el || !this.labels?.length || !this.values?.length) return;

    this.destroy();
    const mod = await import('chart.js');
    const {
      Chart,
      ArcElement,
      BarElement,
      CategoryScale,
      Legend,
      LinearScale,
      Tooltip,
      Title,
      Filler,
      DoughnutController,
      BarController,
      LineController,
      PointElement,
      LineElement,
      PieController,
    } = mod;

    if (!ChartWidgetComponent.chartRegistered) {
      Chart.register(
        ArcElement,
        BarElement,
        CategoryScale,
        Legend,
        LinearScale,
        Tooltip,
        Title,
        Filler,
        DoughnutController,
        BarController,
        LineController,
        PointElement,
        LineElement,
        PieController,
      );
      ChartWidgetComponent.chartRegistered = true;
    }

    const defaultColors =
      this.colors ??
      (this.kind === 'doughnut' || this.kind === 'pie'
        ? ['#6366f1', '#0ea5e9', '#14b8a6', '#8b5cf6', '#f59e0b', '#ec4899', '#64748b', '#22c55e', '#3b82f6']
        : ['#4f46e5', '#0d9488', '#7c3aed', '#ea580c']);

    const layout = this.layoutOptions();

    if (this.kind === 'doughnut' || this.kind === 'pie') {
      this.chart = new Chart(el, {
        type: this.kind === 'pie' ? 'pie' : 'doughnut',
        data: {
          labels: this.labels,
          datasets: [
            {
              label: this.datasetLabel || '—',
              data: this.values,
              backgroundColor: defaultColors.slice(0, this.labels.length),
              borderWidth: 0,
              hoverOffset: 8,
            },
          ],
        },
        options: {
          ...layout,
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            ...layout.plugins,
            legend: {
              ...layout.plugins?.legend,
              position: 'bottom',
              labels: {
                ...layout.plugins?.legend?.labels,
                usePointStyle: true,
                padding: 14,
                font: { size: 11 },
              },
            },
          },
        },
      });
      return;
    }

    if (this.kind === 'line') {
      this.chart = new Chart(el, {
        type: 'line',
        data: {
          labels: this.labels,
          datasets: [
            {
              label: this.datasetLabel || '—',
              data: this.values,
              borderColor: defaultColors[0] ?? '#4f46e5',
              backgroundColor: `${defaultColors[0] ?? '#4f46e5'}22`,
              fill: true,
              tension: 0.35,
              pointRadius: 3,
              pointHoverRadius: 5,
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
              ticks: { font: { size: 11 } },
            },
            y: { beginAtZero: true, grid: { color: 'rgba(148, 163, 184, 0.2)' }, ticks: { precision: 0 } },
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
                padding: 10,
                font: { size: 10 },
              },
            },
          },
        },
      });
      return;
    }

    this.chart = new Chart(el, {
      type: 'bar',
      data: {
        labels: this.labels,
        datasets: [
          {
            label: this.datasetLabel || '—',
            data: this.values,
            backgroundColor: defaultColors.map((c) => `${c}cc`),
            borderRadius: 10,
            borderSkipped: false,
            maxBarThickness: 48,
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
            ticks: { font: { size: 11 } },
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(148, 163, 184, 0.2)' },
            ticks: { precision: 0 },
          },
        },
        plugins: {
          ...layout.plugins,
          legend: { display: false },
        },
      },
    });
  }
}
