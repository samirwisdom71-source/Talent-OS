import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsApiService } from '../../services/notifications-api.service';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly notificationsApi = inject(NotificationsApiService);
  readonly i18n = inject(I18nService);

  readonly unread = signal<number | null>(null);

  ngOnInit(): void {
    this.notificationsApi.getMyUnreadCount().subscribe({
      next: (n) => this.unread.set(n),
      error: () => this.unread.set(null),
    });
  }

  logout(): void {
    this.auth.logout();
  }

  toggleLang(): void {
    this.i18n.toggleLang();
  }
}
