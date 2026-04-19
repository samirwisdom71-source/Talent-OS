import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { ApprovalsApiService } from '../../services/approvals-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { ApprovalRequestDto } from '../../shared/models/approval.models';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { IdChipComponent } from '../../shared/ui/id-chip.component';

const Status = {
  Draft: 1,
  Submitted: 2,
  InReview: 3,
  Approved: 4,
  Rejected: 5,
  Cancelled: 6,
} as const;

type Modal = 'assign' | 'reassign' | 'review' | 'cancel' | null;

@Component({
  selector: 'app-approval-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, IdChipComponent],
  templateUrl: './approval-detail-page.component.html',
  styleUrl: './approval-detail-page.component.scss',
})
export class ApprovalDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApprovalsApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly req = signal<ApprovalRequestDto | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);

  readonly Status = Status;
  readonly PermissionCodes = PermissionCodes;

  readonly modal = signal<Modal>(null);
  assignApproverId = '';
  reassignApproverId = '';
  workflowComments = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reload(id);
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  statusLabel(s: number): string {
    return EnumLabels.approvalStatus(this.lang(), s);
  }

  actionTypeLabel(t: number): string {
    return EnumLabels.approvalActionType(this.lang(), t);
  }

  isRequester(r: ApprovalRequestDto): boolean {
    const uid = this.auth.sessionSnapshot()?.userId;
    if (!uid) return false;
    return uid.toLowerCase() === r.requestedByUserId.toLowerCase();
  }

  open(m: Modal): void {
    this.assignApproverId = '';
    this.reassignApproverId = '';
    this.workflowComments = '';
    this.modal.set(m);
  }

  closeModal(): void {
    this.modal.set(null);
  }

  reload(id: string): void {
    this.api.getById(id).subscribe({
      next: (r) => {
        this.req.set(r);
        this.failed.set(false);
      },
      error: () => {
        this.req.set(null);
        this.failed.set(true);
      },
    });
  }

  submit(): void {
    const id = this.req()?.id;
    if (!id) return;
    this.busy.set(true);
    this.api.submit(id).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.show('تم الإرسال', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإرسال', 'error');
      },
    });
  }

  approve(): void {
    const id = this.req()?.id;
    if (!id) return;
    this.busy.set(true);
    this.api.approve(id, { comments: this.workflowComments || undefined }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.show('تمت الموافقة', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذرت الموافقة', 'error');
      },
    });
  }

  reject(): void {
    const id = this.req()?.id;
    if (!id) return;
    this.busy.set(true);
    this.api.reject(id, { comments: this.workflowComments || undefined }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.show('تم الرفض', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الرفض', 'error');
      },
    });
  }

  confirmAssign(): void {
    const id = this.req()?.id;
    if (!id || !this.assignApproverId.trim()) return;
    this.busy.set(true);
    this.api.assign(id, { approverUserId: this.assignApproverId.trim(), notes: null }).subscribe({
      next: () => {
        this.busy.set(false);
        this.closeModal();
        this.toast.show('تم تعيين المعتمد', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر التعيين', 'error');
      },
    });
  }

  confirmReassign(): void {
    const id = this.req()?.id;
    if (!id || !this.reassignApproverId.trim()) return;
    this.busy.set(true);
    this.api.reassign(id, { newApproverUserId: this.reassignApproverId.trim(), notes: null }).subscribe({
      next: () => {
        this.busy.set(false);
        this.closeModal();
        this.toast.show('تم إعادة التعيين', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذرت إعادة التعيين', 'error');
      },
    });
  }

  confirmStartReview(): void {
    const id = this.req()?.id;
    if (!id) return;
    this.busy.set(true);
    this.api.startReview(id, { comments: this.workflowComments || undefined }).subscribe({
      next: () => {
        this.busy.set(false);
        this.closeModal();
        this.toast.show('بدأت المراجعة', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر بدء المراجعة', 'error');
      },
    });
  }

  confirmCancel(): void {
    const id = this.req()?.id;
    if (!id) return;
    this.busy.set(true);
    this.api.cancel(id, { comments: this.workflowComments || undefined }).subscribe({
      next: () => {
        this.busy.set(false);
        this.closeModal();
        this.toast.show('تم الإلغاء', 'success');
        this.reload(id);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإلغاء', 'error');
      },
    });
  }
}
