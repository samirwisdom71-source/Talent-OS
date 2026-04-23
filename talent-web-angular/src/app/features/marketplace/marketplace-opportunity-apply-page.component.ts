import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { OpportunityApplicationsApiService } from '../../services/opportunity-applications-api.service';
import { UsersApiService } from '../../services/users-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';

@Component({
  selector: 'app-marketplace-opportunity-apply-page',
  standalone: true,
  imports: [RouterLink, FormsModule, LookupSearchComboComponent, TranslatePipe],
  templateUrl: './marketplace-opportunity-apply-page.component.html',
  styleUrl: './marketplace-opportunity-apply-page.component.scss',
})
export class MarketplaceOpportunityApplyPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(OpportunityApplicationsApiService);
  private readonly usersApi = inject(UsersApiService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;
  readonly oppId = signal<string>('');
  readonly busy = signal(false);

  employeeId = '';
  motivation = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.oppId.set(id);

    const uid = this.auth.sessionSnapshot()?.userId;
    if (uid) {
      this.usersApi.getById(uid).subscribe({
        next: (u) => (this.employeeId = u.employeeId ?? ''),
        error: () => (this.employeeId = ''),
      });
    }
  }

  submit(): void {
    const emp = this.employeeId.trim();
    const oid = this.oppId();
    if (!emp || !oid || this.busy()) {
      this.toast.show(this.i18n.t('marketplace.detail.applyNeedEmployee'), 'error');
      return;
    }

    this.busy.set(true);
    this.api
      .apply({
        marketplaceOpportunityId: oid,
        employeeId: emp,
        motivationStatement: this.motivation.trim() || null,
      })
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.show(this.i18n.t('marketplace.detail.toastApplyOk'), 'success');
          void this.router.navigate(['/marketplace/opportunities', oid]);
        },
        error: () => {
          this.busy.set(false);
          this.toast.show(this.i18n.t('marketplace.detail.toastApplyFailed'), 'error');
        },
      });
  }
}
