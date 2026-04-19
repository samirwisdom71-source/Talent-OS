import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ErrorDisplayService } from '../../core/services/error-display.service';
import { LoadingService } from '../../core/services/loading.service';
import { ToastService } from '../../core/services/toast.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent {
  readonly loading = inject(LoadingService);
  readonly errors = inject(ErrorDisplayService);
  readonly toast = inject(ToastService);

  dismissError(): void {
    this.errors.clear();
  }

  dismissToast(): void {
    this.toast.dismiss();
  }
}
