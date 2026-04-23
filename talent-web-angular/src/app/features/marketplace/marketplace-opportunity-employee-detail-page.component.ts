import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { MarketplaceOpportunityDto } from '../../shared/models/marketplace.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-marketplace-opportunity-employee-detail-page',
  standalone: true,
  imports: [RouterLink, DatePipe, TranslatePipe],
  templateUrl: './marketplace-opportunity-employee-detail-page.component.html',
  styleUrl: './marketplace-opportunity-employee-detail-page.component.scss',
})
export class MarketplaceOpportunityEmployeeDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  readonly i18n = inject(I18nService);

  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly opp = signal<MarketplaceOpportunityDto | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.api.getById(id).subscribe({
      next: (row) => {
        this.opp.set(row);
        this.loading.set(false);
        this.failed.set(false);
      },
      error: () => {
        this.opp.set(null);
        this.loading.set(false);
        this.failed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  oppType(t: number): string {
    return EnumLabels.opportunityType(this.lang(), t);
  }
}
