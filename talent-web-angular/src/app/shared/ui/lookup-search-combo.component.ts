import {
  Component,
  ElementRef,
  HostListener,
  effect,
  inject,
  input,
  model,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { LookupItemDto } from '../models/lookup.models';
import { I18nService } from '../services/i18n.service';

/** بحث في لوكاب (موظف، دورة أداء، خطة تعاقب). */
@Component({
  selector: 'app-lookup-search-combo',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="lsc" [class.lsc--open]="panelOpen()">
      @if (label()) {
        <span class="lsc__label">{{ label() }}</span>
      }
      <div class="lsc__wrap">
        <input
          [id]="inputId"
          type="text"
          class="lsc__input"
          [placeholder]="placeholder()"
          [(ngModel)]="inputText"
          (ngModelChange)="onType($event)"
          (focus)="onFocus()"
          autocomplete="off"
        />
        @if (value() && !panelOpen()) {
          <button type="button" class="lsc__clear" (click)="clear($event)" [attr.aria-label]="clearLabel">×</button>
        }
      </div>
      @if (panelOpen()) {
        <ul class="lsc__list" role="listbox">
          @if (loading()) {
            <li class="lsc__hint">{{ loadingLabel }}</li>
          } @else if (!items().length) {
            <li class="lsc__hint">{{ emptyLabel }}</li>
          } @else {
            @for (it of items(); track it.id) {
              <li>
                <button type="button" class="lsc__opt" (mousedown)="pick(it, $event)">
                  <span class="lsc__opt-name">{{ it.name }}</span>
                  @if (it.email) {
                    <span class="lsc__opt-meta">{{ it.email }}</span>
                  }
                </button>
              </li>
            }
          }
        </ul>
      }
    </div>
  `,
  styles: `
    .lsc {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      width: 100%;
      min-width: 0;
    }
    .lsc__label {
      font-size: 0.82rem;
      font-weight: 650;
      color: #334155;
    }
    .lsc__wrap {
      position: relative;
      display: flex;
      align-items: center;
    }
    .lsc__input {
      width: 100%;
      border-radius: 12px;
      border: 1px solid var(--color-border, #e2e8f0);
      padding: 0.62rem 2rem 0.62rem 0.85rem;
      background: #f8fafc;
      font: inherit;
      color: #0f172a;
      transition: border-color 0.15s ease, box-shadow 0.2s ease;
    }
    .lsc__input:focus {
      outline: none;
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.18);
      background: #fff;
    }
    .lsc__clear {
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
    .lsc__clear:hover {
      background: rgba(239, 68, 68, 0.12);
      color: #b91c1c;
    }
    .lsc__list {
      position: absolute;
      z-index: 50;
      left: 0;
      right: 0;
      top: calc(100% + 4px);
      margin: 0;
      padding: 0.35rem;
      list-style: none;
      max-height: min(280px, 42vh);
      overflow-y: auto;
      border-radius: 14px;
      border: 1px solid rgba(148, 163, 184, 0.35);
      background: #fff;
      box-shadow: 0 16px 40px -12px rgba(15, 23, 42, 0.25);
    }
    .lsc__hint {
      padding: 0.65rem 0.75rem;
      font-size: 0.85rem;
      color: var(--color-muted, #64748b);
    }
    .lsc__opt {
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
    .lsc__opt:hover {
      background: rgba(99, 102, 241, 0.08);
    }
    .lsc__opt-name {
      font-weight: 600;
      color: #0f172a;
      font-size: 0.9rem;
    }
    .lsc__opt-meta {
      font-size: 0.76rem;
      color: var(--color-muted, #64748b);
    }
  `,
})
export class LookupSearchComboComponent implements OnInit, OnDestroy {
  private readonly identity = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  private readonly successionApi = inject(SuccessionApiService);
  private readonly i18n = inject(I18nService);
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly kind = input.required<'employee' | 'performanceCycle' | 'successionPlan'>();
  readonly label = input('');
  readonly placeholder = input('');
  readonly value = model<string>('');

  readonly inputId = `lsc-${Math.random().toString(36).slice(2, 9)}`;

  readonly items = signal<LookupItemDto[]>([]);
  readonly loading = signal(false);
  readonly panelOpen = signal(false);

  inputText = '';

  private readonly search$ = new Subject<string>();
  private sub?: Subscription;
  private readonly nameCache = new Map<string, string>();

  constructor() {
    effect(() => {
      const id = this.value();
      if (!id) {
        if (!this.panelOpen()) this.inputText = '';
        return;
      }
      const cached = this.nameCache.get(id);
      if (cached) this.inputText = cached;
    });
  }

  get clearLabel(): string {
    return this.i18n.lang() === 'ar' ? 'مسح' : 'Clear';
  }

  get loadingLabel(): string {
    return this.i18n.t('common.loading');
  }

  get emptyLabel(): string {
    return this.i18n.lang() === 'ar' ? 'لا نتائج' : 'No results';
  }

  ngOnInit(): void {
    this.sub = this.search$
      .pipe(debounceTime(280), distinctUntilChanged())
      .subscribe((q) => this.runSearch(q));

    const v = this.value();
    if (v && !this.nameCache.has(v)) {
      this.fetchLabelForId(v);
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private fetchLabelForId(id: string): void {
    const lang = this.i18n.lang() as 'ar' | 'en';
    if (this.kind() === 'successionPlan') {
      this.successionApi.getSuccessionPlansLookup({ take: 250, search: undefined }).subscribe({
        next: (rows) => {
          const row = rows.find((r) => r.id === id);
          if (row) {
            this.nameCache.set(id, row.name);
            this.inputText = row.name;
          }
        },
      });
      return;
    }
    if (this.kind() === 'employee') {
      this.identity.getEmployees(undefined, 250).subscribe({
        next: (rows) => {
          const row = rows.find((r) => r.id === id);
          if (row) {
            this.nameCache.set(id, row.name);
            this.inputText = row.name;
          }
        },
      });
      return;
    }
    this.cyclesApi.getLookup({ take: 250, lang }).subscribe({
      next: (rows) => {
        const row = rows.find((r) => r.id === id);
        if (row) {
          this.nameCache.set(id, row.name);
          this.inputText = row.name;
        }
      },
    });
  }

  onType(text: string): void {
    if (this.value()) {
      this.value.set('');
    }
    this.panelOpen.set(true);
    this.search$.next(text.trim());
  }

  onFocus(): void {
    this.panelOpen.set(true);
    const q = this.inputText.trim();
    this.search$.next(q);
  }

  private runSearch(query: string): void {
    this.loading.set(true);
    const lang = this.i18n.lang();
    if (this.kind() === 'successionPlan') {
      this.successionApi.getSuccessionPlansLookup({ take: 120, search: query || undefined }).subscribe({
        next: (rows) => {
          this.items.set(rows);
          this.loading.set(false);
        },
        error: () => {
          this.items.set([]);
          this.loading.set(false);
        },
      });
      return;
    }
    if (this.kind() === 'employee') {
      this.identity.getEmployees(query || undefined, 80).subscribe({
        next: (rows) => {
          this.items.set(rows);
          this.loading.set(false);
        },
        error: () => {
          this.items.set([]);
          this.loading.set(false);
        },
      });
      return;
    }
    this.cyclesApi.getLookup({ take: 120, lang: lang as 'ar' | 'en', search: query || undefined }).subscribe({
      next: (rows) => {
        this.items.set(rows);
        this.loading.set(false);
      },
      error: () => {
        this.items.set([]);
        this.loading.set(false);
      },
    });
  }

  pick(it: LookupItemDto, ev: Event): void {
    ev.preventDefault();
    ev.stopPropagation();
    this.nameCache.set(it.id, it.name);
    this.value.set(it.id);
    this.inputText = it.name;
    this.panelOpen.set(false);
    this.items.set([]);
  }

  clear(ev: Event): void {
    ev.stopPropagation();
    ev.preventDefault();
    this.value.set('');
    this.inputText = '';
    this.items.set([]);
    this.panelOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocClick(ev: MouseEvent): void {
    if (!this.host.nativeElement.contains(ev.target as Node)) {
      this.panelOpen.set(false);
      const id = this.value();
      if (id && this.nameCache.has(id)) {
        this.inputText = this.nameCache.get(id)!;
      }
    }
  }
}
