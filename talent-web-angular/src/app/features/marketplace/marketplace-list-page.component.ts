import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DomainAnalyticsApiService } from '../../services/domain-analytics-api.service';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { MarketplaceAnalyticsSummaryDto } from '../../shared/models/domain-analytics.models';
import { MarketplaceOpportunityDto } from '../../shared/models/marketplace.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-marketplace-list-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, TranslatePipe],
  templateUrl: './marketplace-list-page.component.html',
  styleUrl: './marketplace-list-page.component.scss',
})
export class MarketplaceListPageComponent implements OnInit {
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  private readonly analytics = inject(DomainAnalyticsApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<MarketplaceOpportunityDto> | null>(null);
  readonly summary = signal<MarketplaceAnalyticsSummaryDto | null>(null);
  readonly failed = signal(false);
  readonly summaryFailed = signal(false);

  ngOnInit(): void {
    const canViewSummary =
      this.auth.hasPermission(PermissionCodes.MarketplaceManage) || this.auth.hasPermission(PermissionCodes.AnalyticsView);

    forkJoin({
      opps: this.api.getPaged({ page: 1, pageSize: 50 }).pipe(catchError(() => of(null))),
      m: canViewSummary ? this.analytics.getMarketplaceSummary().pipe(catchError(() => of(null))) : of(null),
    }).subscribe({
      next: ({ opps, m }) => {
        this.data.set(opps);
        this.summary.set(m);
        this.failed.set(opps === null);
        this.summaryFailed.set(canViewSummary && m === null);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  oppType(t: number): string {
    return EnumLabels.opportunityType(this.lang(), t);
  }

  oppStatus(s: number): string {
    return EnumLabels.marketplaceOpportunityStatus(this.lang(), s);
  }

  appStatus(s: number): string {
    return EnumLabels.opportunityApplicationStatus(this.lang(), s);
  }
}
