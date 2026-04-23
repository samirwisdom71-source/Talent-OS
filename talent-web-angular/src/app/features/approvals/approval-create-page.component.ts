import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { ApprovalsApiService } from '../../services/approvals-api.service';
import { CreateApprovalRequestRequest } from '../../shared/models/approval.models';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { ApprovalEntityPickerComponent } from './approval-entity-picker.component';

@Component({
  selector: 'app-approval-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe, ApprovalEntityPickerComponent],
  templateUrl: './approval-create-page.component.html',
  styleUrl: './approval-create-page.component.scss',
})
export class ApprovalCreatePageComponent {
  private readonly api = inject(ApprovalsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);
  model: CreateApprovalRequestRequest = {
    requestType: 99,
    relatedEntityId: '',
    title: '',
    summary: null,
    notes: null,
  };

  onRequestTypeChange(next: number): void {
    this.model.requestType = next;
    this.model.relatedEntityId = '';
  }

  save(): void {
    this.busy.set(true);
    this.api.create(this.model).subscribe({
      next: (r) => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('approvals.create.created'), 'success');
        void this.router.navigate(['/approvals', r.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('approvals.create.failed'), 'error');
      },
    });
  }
}
