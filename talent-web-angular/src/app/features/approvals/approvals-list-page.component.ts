import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApprovalsApiService } from '../../services/approvals-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { ApprovalRequestListItemDto } from '../../shared/models/approval.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-approvals-list-page',
  standalone: true,
  imports: [RouterLink, TranslatePipe, DatePipe],
  templateUrl: './approvals-list-page.component.html',
  styleUrl: './approvals-list-page.component.scss',
})
export class ApprovalsListPageComponent implements OnInit {
  private readonly api = inject(ApprovalsApiService);
  private readonly route = inject(ActivatedRoute);
  readonly i18n = inject(I18nService);

  readonly data = signal<PagedResult<ApprovalRequestListItemDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);
  readonly viewMode = signal<ViewMode>('table');
  readonly currentTab = computed(() => this.route.snapshot.routeConfig?.path ?? '');

  ngOnInit(): void {
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  load(): void {
    const path = this.currentTab();
    this.busy.set(true);
    const req$ =
      path === 'submitted'
        ? this.api.getMySubmitted({ page: 1, pageSize: 50 })
        : path === 'assigned'
          ? this.api.getMyAssigned({ page: 1, pageSize: 50 })
          : this.api.getPaged({ page: 1, pageSize: 50 });

    req$.subscribe({
      next: (d) => {
        this.data.set(d);
        this.failed.set(false);
        this.busy.set(false);
      },
      error: () => {
        this.data.set(null);
        this.failed.set(true);
        this.busy.set(false);
      },
    });
  }

  statusLabel(v: number): string {
    return EnumLabels.approvalStatus(this.lang(), v);
  }

  requestTypeLabel(v: number): string {
    return this.i18n.lang() === 'ar' ? `نوع ${v}` : `Type ${v}`;
  }
}
