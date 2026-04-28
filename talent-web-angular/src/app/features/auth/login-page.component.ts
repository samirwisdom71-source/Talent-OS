import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { ApiBusinessError } from '../../shared/utils/api-helpers';

function isApiErrors(value: unknown): value is { errors: string[] } {
  return (
    typeof value === 'object' &&
    value !== null &&
    Array.isArray((value as { errors?: unknown }).errors)
  );
}

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly i18n = inject(I18nService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  error: string | null = null;
  busy = false;
  showPassword = false;

  submit(event?: Event): void {
    event?.preventDefault();
    this.error = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = this.i18n.t('login.validation');
      return;
    }
    this.busy = true;
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (err: unknown) => {
        this.busy = false;
        if (err instanceof ApiBusinessError) {
          this.error = err.errors.join(' · ');
        } else if (err instanceof HttpErrorResponse && isApiErrors(err.error)) {
          this.error = err.error.errors!.join(' · ');
        } else {
          this.error = 'Login failed';
        }
      },
    });
  }

  toggleLang(): void {
    this.i18n.toggleLang();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
