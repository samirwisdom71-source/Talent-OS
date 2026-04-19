import { Component, Input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty" role="status">
      <div class="empty__icon" aria-hidden="true">{{ icon }}</div>
      <h3 class="empty__title">{{ title }}</h3>
      <p class="empty__msg">{{ message }}</p>
      @if (retryLabel) {
        <button type="button" class="btn-primary empty__retry" (click)="retryClick.emit()">{{ retryLabel }}</button>
      }
    </div>
  `,
  styles: `
    .empty {
      text-align: center;
      padding: 2.5rem 1.5rem;
      border-radius: var(--radius-lg, 20px);
      border: 1px dashed var(--color-border);
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.95), rgba(248, 250, 252, 0.9));
    }
    .empty__icon {
      font-size: 2.25rem;
      margin-bottom: 0.75rem;
      filter: grayscale(0.2);
    }
    .empty__title {
      margin: 0 0 0.35rem;
      font-size: 1.1rem;
      font-weight: 650;
    }
    .empty__msg {
      margin: 0 0 1rem;
      color: var(--color-muted);
      font-size: 0.92rem;
      max-width: 28rem;
      margin-inline: auto;
    }
    .empty__retry {
      margin-top: 0.25rem;
    }
  `,
})
export class EmptyStateComponent {
  @Input() icon = '📭';
  @Input({ required: true }) title!: string;
  @Input({ required: true }) message!: string;
  @Input() retryLabel: string | null = null;
  readonly retryClick = output<void>();
}
