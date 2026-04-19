import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApprovalsApiService } from '../../services/approvals-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { ApprovalRequestListItemDto } from '../../shared/models/approval.models';

@Component({
  selector: 'app-approvals-list-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './approvals-list-page.component.html',
  styleUrl: './approvals-list-page.component.scss',
})
export class ApprovalsListPageComponent implements OnInit {
  private readonly api = inject(ApprovalsApiService);
  private readonly route = inject(ActivatedRoute);

  readonly data = signal<PagedResult<ApprovalRequestListItemDto> | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    const path = this.route.snapshot.routeConfig?.path ?? '';
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
      },
      error: () => {
        this.data.set(null);
        this.failed.set(true);
      },
    });
  }
}
