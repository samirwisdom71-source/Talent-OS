import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin } from 'rxjs';
import { DevelopmentPlansApiService } from '../../services/development-plans-api.service';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { DevelopmentAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { DevelopmentPlanDto } from '../../shared/models/development.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { IdChipComponent } from '../../shared/ui/id-chip.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-development-list-page',
  standalone: true,
  imports: [RouterLink, IdChipComponent, DecimalPipe],
  templateUrl: './development-list-page.component.html',
  styleUrl: './development-list-page.component.scss',
})
export class DevelopmentListPageComponent implements OnInit {
  private readonly api = inject(DevelopmentPlansApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<DevelopmentPlanDto> | null>(null);
  readonly summary = signal<DevelopmentAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);

  ngOnInit(): void {
    forkJoin({
      plans: this.api.getPaged({ page: 1, pageSize: 50 }),
      dev: this.analytics.getDevelopmentSummary(),
    }).subscribe({
      next: ({ plans, dev }) => {
        this.data.set(plans);
        this.summary.set(dev);
        this.failed.set(false);
        this.summaryFailed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.summary.set(null);
        this.failed.set(true);
        this.summaryFailed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  planStatus(s: number): string {
    return EnumLabels.developmentPlanStatus(this.lang(), s);
  }
}
