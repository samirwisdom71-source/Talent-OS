import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsApiService } from '../../services/notifications-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { NotificationListItemDto } from '../../shared/models/notification.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { resolveNotificationNavigation } from '../../shared/utils/notification-navigation';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.scss',
})
export class NotificationsPageComponent implements OnInit {
  private readonly api = inject(NotificationsApiService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<NotificationListItemDto> | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getMyPaged({ page: 1, pageSize: 50 }).subscribe({
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

  mark(id: string): void {
    this.api.markRead(id).subscribe({ next: () => this.load() });
  }

  markAll(): void {
    this.api.markAllRead().subscribe({ next: () => this.load() });
  }

  canOpen(row: NotificationListItemDto): boolean {
    return resolveNotificationNavigation(row) !== null;
  }

  open(row: NotificationListItemDto): void {
    const target = resolveNotificationNavigation(row);
    if (!target) {
      return;
    }

    const navigate = () =>
      this.router.navigate(target.commands, {
        queryParams: target.queryParams,
      });

    if (row.isRead) {
      void navigate();
      return;
    }

    this.api.markRead(row.id).subscribe({
      next: () => {
        row.isRead = true;
        void navigate();
      },
      error: () => {
        void navigate();
      },
    });
  }
}
