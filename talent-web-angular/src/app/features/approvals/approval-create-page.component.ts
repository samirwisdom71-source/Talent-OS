import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { ApprovalsApiService } from '../../services/approvals-api.service';
import { CreateApprovalRequestRequest } from '../../shared/models/approval.models';

@Component({
  selector: 'app-approval-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './approval-create-page.component.html',
  styleUrl: '../development/development-create-page.component.scss',
})
export class ApprovalCreatePageComponent {
  private readonly api = inject(ApprovalsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  model: CreateApprovalRequestRequest = {
    requestType: 99,
    relatedEntityId: '',
    title: '',
    summary: null,
    notes: null,
  };

  save(): void {
    this.busy.set(true);
    this.api.create(this.model).subscribe({
      next: (r) => {
        this.busy.set(false);
        this.toast.show('تم إنشاء المسودة', 'success');
        void this.router.navigate(['/approvals', r.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإنشاء', 'error');
      },
    });
  }
}
