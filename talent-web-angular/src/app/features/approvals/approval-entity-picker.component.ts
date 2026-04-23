import {
  Component,
  ElementRef,
  HostListener,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  computed,
  inject,
  model,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { LookupItemDto } from '../../shared/models/lookup.models';
import { I18nService } from '../../shared/services/i18n.service';

interface PickerItem {
  id: string;
  label: string;
  sub?: string;
}

/**
 * Entity picker for approval requests. Based on the chosen requestType it calls
 * the matching lookup endpoint (no permission-gated detail APIs), and lets the
 * user search/pick by a human-readable label. For "Other" (99) it falls back
 * to a plain ID input.
 */
@Component({
  selector: 'app-approval-entity-picker',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="aep" [class.aep--open]="panelOpen()">
      @if (requestType === 99) {
        <input
          type="text"
          class="aep__input"
          [placeholder]="otherHintLabel"
          [(ngModel)]="rawId"
          (ngModelChange)="onRawChange($event)"
        />
      } @else if (!requestType) {
        <input type="text" class="aep__input aep__input--disabled" [placeholder]="pickTypeLabel" disabled />
      } @else {
        <div class="aep__wrap">
          <input
            type="text"
            class="aep__input"
            [placeholder]="placeholder()"
            [(ngModel)]="inputText"
            (ngModelChange)="onType($event)"
            (focus)="onFocus()"
            autocomplete="off"
          />
          @if (value() && !panelOpen()) {
            <button type="button" class="aep__clear" (click)="clear($event)" aria-label="Clear">×</button>
          }
        </div>
        @if (panelOpen()) {
          <ul class="aep__list" role="listbox">
            @if (loading()) {
              <li class="aep__hint">{{ loadingLabel }}</li>
            } @else if (!filtered().length) {
              <li class="aep__hint">{{ emptyLabel }}</li>
            } @else {
              @for (it of filtered(); track it.id) {
                <li>
                  <button type="button" class="aep__opt" (mousedown)="pick(it, $event)">
                    <span class="aep__opt-name">{{ it.label }}</span>
                    @if (it.sub) {
                      <span class="aep__opt-meta">{{ it.sub }}</span>
                    }
                  </button>
                </li>
              }
            }
          </ul>
        }
      }
    </div>
  `,
  styles: `
    .aep {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      min-width: 0;
    }
    .aep__wrap {
      position: relative;
      display: flex;
      align-items: center;
    }
    .aep__input {
      width: 100%;
      border-radius: 12px;
      border: 1px solid var(--color-border, #e2e8f0);
      padding: 0.62rem 2rem 0.62rem 0.85rem;
      background: #f8fafc;
      font: inherit;
      color: #0f172a;
      transition: border-color 0.15s ease, box-shadow 0.2s ease;
    }
    .aep__input:focus {
      outline: none;
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.18);
      background: #fff;
    }
    .aep__input--disabled {
      background: #f1f5f9;
      color: #64748b;
      cursor: not-allowed;
    }
    .aep__clear {
      position: absolute;
      inset-inline-end: 0.35rem;
      top: 50%;
      transform: translateY(-50%);
      border: none;
      background: rgba(15, 23, 42, 0.06);
      width: 28px;
      height: 28px;
      border-radius: 8px;
      cursor: pointer;
      font-size: 1.1rem;
      line-height: 1;
      color: #64748b;
    }
    .aep__clear:hover {
      background: rgba(239, 68, 68, 0.12);
      color: #b91c1c;
    }
    .aep__list {
      position: absolute;
      z-index: 50;
      left: 0;
      right: 0;
      top: calc(100% + 4px);
      margin: 0;
      padding: 0.35rem;
      list-style: none;
      max-height: min(300px, 44vh);
      overflow-y: auto;
      border-radius: 14px;
      border: 1px solid rgba(148, 163, 184, 0.35);
      background: #fff;
      box-shadow: 0 16px 40px -12px rgba(15, 23, 42, 0.25);
    }
    .aep__hint {
      padding: 0.65rem 0.75rem;
      font-size: 0.85rem;
      color: var(--color-muted, #64748b);
    }
    .aep__opt {
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 0.12rem;
      padding: 0.55rem 0.65rem;
      border: none;
      border-radius: 10px;
      background: transparent;
      cursor: pointer;
      font: inherit;
      text-align: start;
      transition: background 0.12s ease;
    }
    .aep__opt:hover {
      background: rgba(99, 102, 241, 0.08);
    }
    .aep__opt-name {
      font-weight: 600;
      color: #0f172a;
      font-size: 0.9rem;
    }
    .aep__opt-meta {
      font-size: 0.76rem;
      color: var(--color-muted, #64748b);
    }
  `,
})
export class ApprovalEntityPickerComponent implements OnInit, OnChanges {
  private readonly identity = inject(IdentityLookupsApiService);
  private readonly succession = inject(SuccessionApiService);
  private readonly i18n = inject(I18nService);
  private readonly host = inject(ElementRef<HTMLElement>);

  @Input() requestType: number | null = null;
  readonly value = model<string>('');
  readonly placeholder = signal<string>('');

  readonly items = signal<PickerItem[]>([]);
  readonly loading = signal(false);
  readonly panelOpen = signal(false);
  inputText = '';
  rawId = '';

  readonly filtered = computed<PickerItem[]>(() => {
    const q = (this.inputText || '').trim().toLowerCase();
    const all = this.items();
    if (!q) return all;
    return all.filter(
      (i) => i.label.toLowerCase().includes(q) || (i.sub ?? '').toLowerCase().includes(q),
    );
  });

  private nameById = new Map<string, string>();

  ngOnInit(): void {
    this.placeholder.set(this.i18n.t('approvals.create.relatedEntity.placeholder'));
    if (this.requestType && this.requestType !== 99) {
      this.loadForType(this.requestType);
    }
    if (this.requestType === 99 && this.value()) {
      this.rawId = this.value();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['requestType']) {
      const prev = changes['requestType'].previousValue;
      const next = changes['requestType'].currentValue;
      if (prev === next) return;
      this.items.set([]);
      this.inputText = '';
      this.value.set('');
      this.rawId = '';
      if (next && next !== 99) {
        this.loadForType(next);
      }
    }
  }

  get loadingLabel(): string {
    return this.i18n.t('common.loading');
  }
  get emptyLabel(): string {
    return this.i18n.lang() === 'ar' ? 'لا نتائج' : 'No results';
  }
  get otherHintLabel(): string {
    return this.i18n.t('approvals.create.relatedEntity.otherHint');
  }
  get pickTypeLabel(): string {
    return this.i18n.t('approvals.create.pickType');
  }

  onType(_text: string): void {
    if (this.value()) this.value.set('');
    this.panelOpen.set(true);
  }

  onFocus(): void {
    this.panelOpen.set(true);
  }

  onRawChange(text: string): void {
    this.value.set((text || '').trim());
  }

  pick(it: PickerItem, ev: Event): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.nameById.set(it.id, it.label);
    this.value.set(it.id);
    this.inputText = it.label;
    this.panelOpen.set(false);
  }

  clear(ev: Event): void {
    ev.stopPropagation();
    ev.preventDefault();
    this.value.set('');
    this.inputText = '';
    this.panelOpen.set(true);
  }

  @HostListener('document:click', ['$event'])
  onDocClick(ev: MouseEvent): void {
    if (!this.host.nativeElement.contains(ev.target as Node)) {
      this.panelOpen.set(false);
      const id = this.value();
      if (id && this.nameById.has(id)) {
        this.inputText = this.nameById.get(id)!;
      }
    }
  }

  private loadForType(rt: number): void {
    this.loading.set(true);
    const source$ = this.resolveLookup(rt);
    if (!source$) {
      this.items.set([]);
      this.loading.set(false);
      return;
    }
    source$.subscribe({
      next: (rows) => {
        const items: PickerItem[] = (rows ?? []).map((r) => ({
          id: r.id,
          label: r.name,
          sub: r.email ?? undefined,
        }));
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.items.set([]);
        this.loading.set(false);
      },
    });
  }

  private displayLang(): 'ar' | 'en' {
    return this.i18n.lang() === 'en' ? 'en' : 'ar';
  }

  private resolveLookup(rt: number): Observable<LookupItemDto[]> | null {
    const take = 250;
    const lang = this.displayLang();
    switch (rt) {
      case 1:
        return this.identity.getPerformanceEvaluations(undefined, take, lang);
      case 2:
        return this.identity.getTalentClassifications(undefined, take, lang);
      case 3:
        return this.succession.getSuccessionPlansLookup({ take });
      case 4:
        return this.identity.getDevelopmentPlans(undefined, take, lang);
      case 5:
        return this.identity.getMarketplaceOpportunities(undefined, take, lang);
      case 6:
        return this.identity.getOpportunityApplications(undefined, take, lang);
      default:
        return null;
    }
  }
}
