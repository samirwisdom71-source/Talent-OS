import { DatePipe } from '@angular/common';
import { Component, ElementRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsApiService } from '../../services/notifications-api.service';
import { NotificationListItemDto } from '../../shared/models/notification.models';
import { resolveNotificationNavigation } from '../../shared/utils/notification-navigation';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { LayoutStateService } from '../layout-state.service';
import { PermissionCodes } from '../../shared/models/permission-codes';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [RouterLink, TranslatePipe, DatePipe],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly notificationsApi = inject(NotificationsApiService);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);
  readonly layout = inject(LayoutStateService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  readonly unread = signal<number | null>(null);
  readonly notificationsOpen = signal(false);
  readonly profileOpen = signal(false);
  readonly notifications = signal<NotificationListItemDto[]>([]);
  readonly notificationsBusy = signal(false);

  ngOnInit(): void {
    if (!this.auth.hasPermission(PermissionCodes.NotificationView)) {
      return;
    }
    this.loadUnread();
  }

  private loadUnread(): void {
    this.notificationsApi.getMyUnreadCount().subscribe({
      next: (n) => this.unread.set(n),
      error: () => this.unread.set(null),
    });
  }

  toggleNotifications(): void {
    if (!this.auth.hasPermission(PermissionCodes.NotificationView)) {
      return;
    }
    const next = !this.notificationsOpen();
    this.notificationsOpen.set(next);
    this.profileOpen.set(false);
    if (next) {
      this.loadNotifications();
    }
  }

  toggleProfile(): void {
    this.profileOpen.update((v) => !v);
    this.notificationsOpen.set(false);
  }

  private loadNotifications(): void {
    this.notificationsBusy.set(true);
    this.notificationsApi.getMyPaged({ page: 1, pageSize: 8 }).subscribe({
      next: (res) => {
        this.notifications.set([...res.items]);
        this.notificationsBusy.set(false);
      },
      error: () => {
        this.notifications.set([]);
        this.notificationsBusy.set(false);
      },
    });
  }

  markRead(id: string): void {
    this.notificationsApi.markRead(id).subscribe({
      next: () => {
        this.notifications.update((rows) => rows.filter((x) => x.id !== id));
        this.loadUnread();
      },
    });
  }

  markAllRead(): void {
    this.notificationsApi.markAllRead().subscribe({
      next: () => {
        this.notifications.set([]);
        this.unread.set(0);
      },
    });
  }

  canOpen(item: NotificationListItemDto): boolean {
    return resolveNotificationNavigation(item) !== null;
  }

  openNotification(item: NotificationListItemDto): void {
    const target = resolveNotificationNavigation(item);
    if (!target) {
      return;
    }

    const go = () => {
      this.notificationsOpen.set(false);
      void this.router.navigate(target.commands, { queryParams: target.queryParams });
    };

    if (item.isRead) {
      go();
      return;
    }

    this.notificationsApi.markRead(item.id).subscribe({
      next: () => {
        this.notifications.update((rows) => rows.filter((x) => x.id !== item.id));
        this.loadUnread();
        go();
      },
      error: () => go(),
    });
  }

  logout(): void {
    this.auth.logout();
  }

  toggleLang(): void {
    this.i18n.toggleLang();
  }

  toggleMobileSidebar(): void {
    this.layout.toggleMobileSidebar();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.notificationsOpen.set(false);
      this.profileOpen.set(false);
    }
  }
}
