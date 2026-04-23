import { DatePipe } from '@angular/common';
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
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { LookupItemDto } from '../../shared/models/lookup.models';

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
  imports: [RouterLink, FormsModule, DatePipe, TranslatePipe, LookupSearchComboComponent],
  templateUrl: './approval-detail-page.component.html',
  styleUrl: './approval-detail-page.component.scss',
})
export class ApprovalDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApprovalsApiService);
  private readonly identity = inject(IdentityLookupsApiService);
  private readonly succession = inject(SuccessionApiService);
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
  readonly users = signal<LookupItemDto[]>([]);

  readonly relatedEntityTitle = signal<string>('');
  readonly relatedEntitySub = signal<string | null>(null);
  readonly relatedEntityLoading = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reload(id);
  }

  private displayLang(): 'ar' | 'en' {
    return this.i18n.lang() === 'en' ? 'en' : 'ar';
  }

  private loadUsers(): void {
    this.identity.getUsers(undefined, 400, this.displayLang()).subscribe({
      next: (rows) => this.users.set(rows),
      error: () => this.users.set([]),
    });
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

  requestTypeLabel(v: number): string {
    const key = `approvals.type.${v}`;
    const t = this.i18n.t(key);
    if (t === key) {
      return this.i18n.lang() === 'ar' ? `نوع ${v}` : `Type ${v}`;
    }
    return t;
  }

  userName(id: string | null | undefined): string {
    if (!id) return '—';
    return this.users().find((x) => x.id.toLowerCase() === id.toLowerCase())?.name ?? '—';
  }

  isRequester(r: ApprovalRequestDto): boolean {
    const uid = this.auth.sessionSnapshot()?.userId;
    if (!uid) return false;
    return uid.toLowerCase() === r.requestedByUserId.toLowerCase();
  }

  /** يطابق الـ API: فقط المعتمد الحالي يمكنه بدء المراجعة / الموافقة / الرفض. */
  isCurrentApprover(r: ApprovalRequestDto): boolean {
    const uid = this.auth.sessionSnapshot()?.userId;
    if (!uid || !r.currentApproverUserId) return false;
    return uid.toLowerCase() === r.currentApproverUserId.toLowerCase();
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
        this.loadUsers();
        this.loadRelatedEntityDisplay(r);
      },
      error: () => {
        this.req.set(null);
        this.failed.set(true);
      },
    });
  }

  private loadRelatedEntityDisplay(r: ApprovalRequestDto): void {
    this.relatedEntityLoading.set(true);
    this.relatedEntityTitle.set('');
    this.relatedEntitySub.set(null);

    const rawId = (r.relatedEntityId || '').trim();
    if (!rawId) {
      this.relatedEntityTitle.set('—');
      this.relatedEntityLoading.set(false);
      return;
    }

    if (r.requestType === 99) {
      this.relatedEntityTitle.set(rawId);
      this.relatedEntityLoading.set(false);
      return;
    }

    const lang = this.displayLang();
    const take = 500;
    const apply = (row: LookupItemDto | undefined): void => {
      const title = row?.name?.trim();
      const sub = row?.email?.trim();
      this.relatedEntityTitle.set(title || rawId);
      this.relatedEntitySub.set(sub && sub.length > 0 ? sub : null);
      this.relatedEntityLoading.set(false);
    };
    const fail = (): void => {
      this.relatedEntityTitle.set(rawId);
      this.relatedEntitySub.set(null);
      this.relatedEntityLoading.set(false);
    };
    const find = (rows: LookupItemDto[]): LookupItemDto | undefined =>
      rows.find((x) => x.id.toLowerCase() === rawId.toLowerCase());

    switch (r.requestType) {
      case 1:
        this.identity.getPerformanceEvaluations(undefined, take, lang).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      case 2:
        this.identity.getTalentClassifications(undefined, take, lang).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      case 3:
        this.succession.getSuccessionPlansLookup({ take }).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      case 4:
        this.identity.getDevelopmentPlans(undefined, take, lang).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      case 5:
        this.identity.getMarketplaceOpportunities(undefined, take, lang).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      case 6:
        this.identity.getOpportunityApplications(undefined, take, lang).subscribe({
          next: (rows) => apply(find(rows)),
          error: fail,
        });
        break;
      default:
        this.relatedEntityTitle.set(rawId);
        this.relatedEntityLoading.set(false);
    }
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
