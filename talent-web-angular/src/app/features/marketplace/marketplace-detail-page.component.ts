import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { MarketplaceOpportunitiesApiService } from '../../services/marketplace-opportunities-api.service';
import { OpportunityApplicationsApiService } from '../../services/opportunity-applications-api.service';
import { OpportunityApplicationDto } from '../../shared/models/opportunity-application.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { MarketplaceOpportunityDto } from '../../shared/models/marketplace.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

const OppStatus = {
  Draft: 1,
  Open: 2,
  Closed: 3,
  Cancelled: 4,
} as const;

const AppStatus = {
  Submitted: 1,
  UnderReview: 2,
  Shortlisted: 3,
  Accepted: 4,
  Rejected: 5,
  Withdrawn: 6,
} as const;

@Component({
  selector: 'app-marketplace-detail-page',
  standalone: true,
  imports: [RouterLink, DatePipe, TranslatePipe],
  templateUrl: './marketplace-detail-page.component.html',
  styleUrl: './marketplace-detail-page.component.scss',
})
export class MarketplaceDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(MarketplaceOpportunitiesApiService);
  private readonly appsApi = inject(OpportunityApplicationsApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;
  readonly OppStatus = OppStatus;
  readonly AppStatus = AppStatus;

  readonly opp = signal<MarketplaceOpportunityDto | null>(null);
  readonly applications = signal<readonly OpportunityApplicationDto[]>([]);
  readonly failed = signal(false);
  readonly busyOpp = signal(false);
  readonly busyRowId = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reload(id);
  }

  private runOppAction(action: Observable<unknown>): void {
    const oid = this.opp()?.id;
    if (!oid) return;
    this.busyOpp.set(true);
    action.subscribe({
      next: () => {
        this.busyOpp.set(false);
        this.toast.show(this.i18n.t('marketplace.detail.toastUpdated'), 'success');
        this.reload(oid);
      },
      error: () => {
        this.busyOpp.set(false);
        this.toast.show(this.i18n.t('marketplace.detail.toastActionFailed'), 'error');
      },
    });
  }

  openOpp(): void {
    const o = this.opp();
    if (!o) return;
    this.runOppAction(this.api.open(o.id));
  }

  closeOpp(): void {
    const o = this.opp();
    if (!o) return;
    this.runOppAction(this.api.close(o.id));
  }

  cancelOpp(): void {
    const o = this.opp();
    if (!o) return;
    this.runOppAction(this.api.cancel(o.id));
  }

  private runAppAction(id: string, action: Observable<unknown>): void {
    const oid = this.opp()?.id;
    if (!oid) return;
    this.busyRowId.set(id);
    action.subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('marketplace.detail.toastUpdated'), 'success');
        this.reload(oid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('marketplace.detail.toastActionFailed'), 'error');
      },
    });
  }

  underReview(a: OpportunityApplicationDto): void {
    this.runAppAction(a.id, this.appsApi.markUnderReview(a.id));
  }

  shortlist(a: OpportunityApplicationDto): void {
    this.runAppAction(a.id, this.appsApi.shortlist(a.id));
  }

  accept(a: OpportunityApplicationDto): void {
    this.runAppAction(a.id, this.appsApi.accept(a.id));
  }

  reject(a: OpportunityApplicationDto): void {
    this.runAppAction(a.id, this.appsApi.reject(a.id));
  }

  reload(id: string): void {
    forkJoin({
      opp: this.api.getById(id),
      apps: this.appsApi.getPaged({ page: 1, pageSize: 200, marketplaceOpportunityId: id }),
    }).subscribe({
      next: ({ opp, apps }) => {
        this.opp.set(opp);
        this.applications.set(apps.items);
        this.failed.set(false);
      },
      error: () => {
        this.opp.set(null);
        this.applications.set([]);
        this.failed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  oppStatus(s: number): string {
    return EnumLabels.marketplaceOpportunityStatus(this.lang(), s);
  }

  oppType(t: number): string {
    return EnumLabels.opportunityType(this.lang(), t);
  }

  appStatus(s: number): string {
    return EnumLabels.opportunityApplicationStatus(this.lang(), s);
  }
}
