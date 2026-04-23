import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { CreateMarketplaceOpportunityRequest } from '../../shared/models/marketplace.models';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-marketplace-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule, LookupSearchComboComponent, TranslatePipe],
  templateUrl: './marketplace-create-page.component.html',
  styleUrl: './marketplace-create-page.component.scss',
})
export class MarketplaceCreatePageComponent implements OnInit {
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  readonly opportunityTypes = [1, 2, 3, 4, 5, 6, 7, 8] as const;

  /** Mirror for optional position; cleared when OU changes via combo parent input. */
  positionComboId = '';

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

  lang(): UiLang {
    return this.i18n.lang();
  }

  opportunityTypeLabel(t: number): string {
    return EnumLabels.opportunityType(this.lang(), t);
  }

  save(): void {
    if (!this.model.organizationUnitId?.trim()) {
      this.toast.show(this.i18n.t('marketplace.create.toastPickUnit'), 'error');
      return;
    }
    if (!this.model.title?.trim()) {
      this.toast.show(this.i18n.t('marketplace.create.toastTitle'), 'error');
      return;
    }

    this.busy.set(true);
    const positionId =
      this.positionComboId && String(this.positionComboId).trim()
        ? String(this.positionComboId).trim()
        : null;

    const body: CreateMarketplaceOpportunityRequest = {
      ...this.model,
      organizationUnitId: this.model.organizationUnitId.trim(),
      openDate: this.model.openDate ? `${this.model.openDate}T00:00:00Z` : new Date().toISOString(),
      positionId,
      closeDate:
        this.model.closeDate && String(this.model.closeDate).trim()
          ? `${String(this.model.closeDate).trim()}T00:00:00Z`
          : null,
    };

    this.api.create(body).subscribe({
      next: (o) => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('marketplace.create.toastCreated'), 'success');
        void this.router.navigate(['/marketplace', o.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('marketplace.create.toastFailed'), 'error');
      },
    });
  }
}
