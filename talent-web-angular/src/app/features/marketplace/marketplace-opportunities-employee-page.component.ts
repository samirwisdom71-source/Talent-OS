import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { MarketplaceOpportunityDto } from '../../shared/models/marketplace.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-marketplace-opportunities-employee-page',
  standalone: true,
  imports: [RouterLink, DatePipe, FormsModule, TranslatePipe],
  templateUrl: './marketplace-opportunities-employee-page.component.html',
  styleUrl: './marketplace-opportunities-employee-page.component.scss',
})
export class MarketplaceOpportunitiesEmployeePageComponent implements OnInit {
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  readonly i18n = inject(I18nService);

  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly all = signal<readonly MarketplaceOpportunityDto[]>([]);
  readonly query = signal('');
  readonly mode = signal<'cards' | 'table'>('cards');

  readonly filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    const rows = this.all();
    if (!q) return rows;
    return rows.filter((x) => {
      const hay = `${x.title} ${x.description ?? ''} ${x.organizationUnitName ?? ''} ${x.positionTitle ?? ''}`.toLowerCase();
      return hay.includes(q);
    });
  });

  ngOnInit(): void {
    this.api.getPaged({ page: 1, pageSize: 200, status: 2 }).subscribe({
      next: (p) => {
        this.all.set(p.items);
        this.loading.set(false);
        this.failed.set(false);
      },
      error: () => {
        this.all.set([]);
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
