import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { TalentClassificationsApiService } from '../../services/talent-classifications-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { TalentDistributionSummaryDto } from '../../shared/models/domain-analytics.models';
import { TalentClassificationDto } from '../../shared/models/classification.models';
import { PerformanceCycleDto } from '../../shared/models/performance.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { IdChipComponent } from '../../shared/ui/id-chip.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-nine-box-page',
  standalone: true,
  imports: [FormsModule, IdChipComponent, TranslatePipe],
  templateUrl: './nine-box-page.component.html',
  styleUrls: ['./nine-box-page.component.scss', './talent-pages.component.scss'],
})
export class NineBoxPageComponent implements OnInit {
  private readonly api = inject(TalentClassificationsApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  readonly i18n = inject(I18nService);

  /** '' = all boxes */
  filterBox = '';
  /** '' = all cycles */
  cycleFilterId = '';
  page = 1;
  readonly pageSize = 25;

  readonly cycles = signal<readonly PerformanceCycleDto[]>([]);
  readonly data = signal<PagedResult<TalentClassificationDto> | null>(null);
  readonly failed = signal(false);
  readonly talentSummary = signal<TalentDistributionSummaryDto | null>(null);
  readonly summaryFailed = signal(false);

  ngOnInit(): void {
    this.cyclesApi.getPaged({ page: 1, pageSize: 100 }).subscribe({
      next: (p) => this.cycles.set(p.items),
      error: () => this.cycles.set([]),
    });
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  nineBoxLabel(code: number): string {
    return EnumLabels.nineBoxCode(this.lang(), code);
  }

  cycleLabel(c: PerformanceCycleDto): string {
    return this.i18n.lang() === 'ar' ? c.nameAr || c.nameEn : c.nameEn || c.nameAr;
  }

  load(): void {
    this.failed.set(false);
    this.summaryFailed.set(false);
    const nine = this.filterBox === '' ? null : Number(this.filterBox);
    const cycleId = this.cycleFilterId || null;

    forkJoin({
      rows: this.api.getPaged({
        page: this.page,
        pageSize: this.pageSize,
        performanceCycleId: cycleId,
        nineBoxCode: nine === null || Number.isNaN(nine) ? null : nine,
      }),
      dist: this.analytics.getTalentDistribution({
        performanceCycleId: cycleId,
        organizationUnitId: null,
      }),
    }).subscribe({
      next: ({ rows, dist }) => {
        this.data.set(rows);
        this.talentSummary.set(dist);
        this.failed.set(false);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.talentSummary.set(null);
        this.failed.set(true);
        this.summaryFailed.set(true);
      },
    });
  }

  next(): void {
    const d = this.data();
    if (!d?.hasNextPage) return;
    this.page += 1;
    this.load();
  }

  prev(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }

  onFiltersChange(): void {
    this.page = 1;
    this.load();
  }
}
