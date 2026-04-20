import { DatePipe } from '@angular/common';
import { Component, ElementRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsApiService } from '../../services/notifications-api.service';
import { NotificationListItemDto } from '../../shared/models/notification.models';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { LayoutStateService } from '../layout-state.service';

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
  private readonly host = inject(ElementRef<HTMLElement>);
  readonly layout = inject(LayoutStateService);
  readonly i18n = inject(I18nService);

  readonly unread = signal<number | null>(null);
  readonly notificationsOpen = signal(false);
  readonly profileOpen = signal(false);
  readonly notifications = signal<NotificationListItemDto[]>([]);
  readonly notificationsBusy = signal(false);

  ngOnInit(): void {
    this.loadUnread();
  }

  private loadUnread(): void {
    this.notificationsApi.getMyUnreadCount().subscribe({
      next: (n) => this.unread.set(n),
      error: () => this.unread.set(null),
    });
  }

  toggleNotifications(): void {
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
