import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { CreateMarketplaceOpportunityRequest } from '../../shared/models/marketplace.models';

@Component({
  selector: 'app-marketplace-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './marketplace-create-page.component.html',
  styleUrl: '../development/development-create-page.component.scss',
})
export class MarketplaceCreatePageComponent implements OnInit {
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  model: CreateMarketplaceOpportunityRequest = {
    title: '',
    description: null,
    opportunityType: 1,
    organizationUnitId: '',
    positionId: null,
    requiredCompetencySummary: null,
    openDate: '',
    closeDate: null,
    maxApplicants: null,
    isConfidential: false,
    notes: null,
  };

  ngOnInit(): void {
    const d = new Date();
    this.model.openDate = d.toISOString().slice(0, 10);
  }

  save(): void {
    this.busy.set(true);
    const body: CreateMarketplaceOpportunityRequest = {
      ...this.model,
      openDate: this.model.openDate ? `${this.model.openDate}T00:00:00Z` : new Date().toISOString(),
      positionId: this.model.positionId && String(this.model.positionId).trim() ? String(this.model.positionId).trim() : null,
      closeDate:
        this.model.closeDate && String(this.model.closeDate).trim()
          ? `${String(this.model.closeDate).trim()}T00:00:00Z`
          : null,
    };
    this.api.create(body).subscribe({
      next: (o) => {
        this.busy.set(false);
        this.toast.show('تم إنشاء الفرصة', 'success');
        void this.router.navigate(['/marketplace', o.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإنشاء', 'error');
      },
    });
  }
}
