import { Component, Input, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-id-chip',
  standalone: true,
  template: `
    <span class="id-chip" [attr.title]="id">
      @if (label) {
        <span class="id-chip__lbl">{{ label }}</span>
      }
      <code class="id-chip__code">{{ display }}</code>
      <button type="button" class="id-chip__btn" (click)="copy()" [disabled]="!id">نسخ</button>
    </span>
  `,
  styles: `
    .id-chip {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      flex-wrap: wrap;
      font-size: 0.82rem;
    }
    .id-chip__lbl {
      color: var(--color-muted);
      font-size: 0.72rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .id-chip__code {
      background: rgba(15, 23, 42, 0.06);
      padding: 0.12rem 0.4rem;
      border-radius: 6px;
      font-size: 0.78rem;
    }
    .id-chip__btn {
      border: 1px solid var(--color-border);
      background: #fff;
      border-radius: 6px;
      padding: 0.1rem 0.45rem;
      font-size: 0.72rem;
      cursor: pointer;
    }
    .id-chip__btn:disabled {
      opacity: 0.45;
      cursor: not-allowed;
    }
  `,
})
export class IdChipComponent {
  private readonly toast = inject(ToastService);

  @Input() label = '';
  @Input() id: string | null | undefined = '';

  get display(): string {
    const v = this.id ?? '';
    if (!v) return '—';
    if (v.length <= 14) return v;
    return `${v.slice(0, 8)}…${v.slice(-4)}`;
  }

  copy(): void {
    const v = this.id ?? '';
    if (!v || !navigator.clipboard) return;
    void navigator.clipboard.writeText(v).then(() => this.toast.show('تم نسخ المعرّف', 'success'));
  }
}
