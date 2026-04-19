import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsApiService } from '../../services/notifications-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { NotificationListItemDto } from '../../shared/models/notification.models';
import { PermissionCodes } from '../../shared/models/permission-codes';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.scss',
})
export class NotificationsPageComponent implements OnInit {
  private readonly api = inject(NotificationsApiService);
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
}
