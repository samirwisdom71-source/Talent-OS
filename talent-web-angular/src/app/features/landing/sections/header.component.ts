import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-landing-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private readonly auth = inject(AuthService);

  @Input({ required: true }) lang!: 'en' | 'ar';
  @Input({ required: true }) nav!: {
    platform: string;
    modules: string;
    outcomes: string;
    how: string;
    demo: string;
    signIn: string;
    request: string;
  };
  @Output() readonly langChange = new EventEmitter<'en' | 'ar'>();
  @Output() readonly navigateTo = new EventEmitter<string>();

  isAuthenticated(): boolean {
    return this.auth.isAuthenticated();
  }
}
