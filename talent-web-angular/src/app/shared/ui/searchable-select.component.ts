import {
  Component,
  ElementRef,
  HostListener,
  computed,
  forwardRef,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SearchableSelectItem {
  id: string;
  name: string;
}

@Component({
  selector: 'app-searchable-select',
  standalone: true,
  imports: [FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchableSelectComponent),
      multi: true,
    },
  ],
  template: `
    <div class="sselect" [class.sselect--open]="open()">
      <button
        type="button"
        class="sselect__trigger"
        (click)="toggle()"
        [disabled]="disabled()"
        [attr.aria-expanded]="open()"
      >
        <span class="sselect__value" [class.sselect__value--placeholder]="!selectedLabel()">
          {{ selectedLabel() || placeholder() }}
        </span>
        <svg class="sselect__chev" viewBox="0 0 20 20" aria-hidden="true">
          <path d="M5 8l5 5 5-5" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </button>

      @if (open()) {
        <div class="sselect__panel" role="listbox">
          <div class="sselect__search">
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <circle cx="11" cy="11" r="7" fill="none" stroke="currentColor" stroke-width="2" />
              <path d="M20 20l-3.5-3.5" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
            <input
              #searchInput
              type="search"
              [ngModel]="searchTerm()"
              (ngModelChange)="onSearchChange($event)"
              [placeholder]="searchPlaceholder()"
              autocomplete="off"
            />
          </div>

          <ul class="sselect__list">
            @if (showAllOption()) {
              <li
                class="sselect__item sselect__item--all"
                [class.sselect__item--selected]="!value()"
                (mousedown)="select('')"
              >
                {{ allLabel() }}
              </li>
            }
            @if (filteredItems().length === 0) {
              <li class="sselect__empty">لا توجد نتائج</li>
            }
            @for (item of filteredItems(); track item.id) {
              <li
                class="sselect__item"
                [class.sselect__item--selected]="value() === item.id"
                (mousedown)="select(item.id)"
              >
                {{ item.name }}
              </li>
            }
          </ul>
        </div>
      }
    </div>
  `,
  styles: `
    :host { display: block; width: 100%; }
    .sselect { position: relative; }
    .sselect__trigger {
      width: 100%;
      display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
      border-radius: 12px;
      border: 1px solid var(--color-border);
      background: #fff;
      padding: 0.55rem 0.85rem;
      cursor: pointer;
      font-size: 0.9rem;
      color: #0f172a;
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }
    .sselect__trigger:hover { border-color: #94a3b8; }
    .sselect__trigger:focus-visible {
      outline: none; border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.18);
    }
    .sselect--open .sselect__trigger {
      border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.14);
    }
    .sselect__value { flex: 1; text-align: start; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .sselect__value--placeholder { color: var(--color-muted); font-weight: 400; }
    .sselect__chev { width: 16px; height: 16px; color: #64748b; flex-shrink: 0; transition: transform 0.15s ease; }
    .sselect--open .sselect__chev { transform: rotate(180deg); }

    .sselect__panel {
      position: absolute; inset-inline-start: 0; inset-inline-end: 0; top: calc(100% + 6px);
      z-index: 60;
      background: #fff;
      border: 1px solid rgba(148, 163, 184, 0.3);
      border-radius: 14px;
      box-shadow: 0 12px 38px -14px rgba(15, 23, 42, 0.22);
      padding: 0.55rem;
      max-height: 320px;
      display: flex; flex-direction: column; gap: 0.5rem;
    }
    .sselect__search {
      position: relative; display: flex; align-items: center;
      background: #f8fafc;
      border-radius: 10px;
      padding: 0 0.6rem;
      border: 1px solid rgba(148, 163, 184, 0.3);
    }
    .sselect__search svg { width: 16px; height: 16px; color: #64748b; flex-shrink: 0; }
    .sselect__search input {
      flex: 1;
      border: none; background: transparent; outline: none;
      padding: 0.5rem 0.4rem;
      font-size: 0.88rem;
      font-family: inherit;
    }
    .sselect__search input::-webkit-search-cancel-button { appearance: none; }

    .sselect__list {
      list-style: none; margin: 0; padding: 0;
      overflow-y: auto; max-height: 240px;
      display: flex; flex-direction: column; gap: 2px;
    }
    .sselect__item {
      padding: 0.5rem 0.7rem;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.88rem;
      color: #1e293b;
      transition: background 0.1s ease, color 0.1s ease;
    }
    .sselect__item:hover { background: #eef2ff; color: #3730a3; }
    .sselect__item--selected {
      background: linear-gradient(135deg, rgba(99, 102, 241, 0.15), rgba(14, 165, 233, 0.12));
      color: #3730a3; font-weight: 600;
    }
    .sselect__item--all { color: #64748b; font-weight: 500; border-bottom: 1px dashed rgba(148, 163, 184, 0.35); border-radius: 0; padding-bottom: 0.6rem; margin-bottom: 0.1rem; }
    .sselect__empty { padding: 0.75rem; text-align: center; color: var(--color-muted); font-size: 0.85rem; }
  `,
})
export class SearchableSelectComponent implements ControlValueAccessor {
  readonly items = input<readonly SearchableSelectItem[]>([]);
  readonly placeholder = input<string>('اختر...');
  readonly searchPlaceholder = input<string>('ابحث...');
  readonly allLabel = input<string>('الكل');
  readonly showAllOption = input<boolean>(true);

  readonly searchChange = output<string>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly value = signal<string>('');
  readonly open = signal(false);
  readonly disabled = signal(false);
  readonly searchTerm = signal<string>('');

  readonly filteredItems = computed(() => {
    const q = this.searchTerm().trim().toLowerCase();
    const list = this.items();
    if (!q) return list;
    return list.filter(
      (x) =>
        x.name.toLowerCase().includes(q) ||
        x.id.toLowerCase().includes(q),
    );
  });

  readonly selectedLabel = computed(() => {
    const id = this.value();
    if (!id) return '';
    return this.items().find((x) => x.id === id)?.name ?? '';
  });

  private onChange: (v: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(v: string | null): void {
    this.value.set(v ?? '');
  }
  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  toggle(): void {
    if (this.disabled()) return;
    this.open.update((v) => !v);
    if (this.open()) {
      this.searchTerm.set('');
    } else {
      this.onTouched();
    }
  }

  select(id: string): void {
    this.value.set(id);
    this.onChange(id);
    this.open.set(false);
    this.onTouched();
  }

  onSearchChange(v: string): void {
    this.searchTerm.set(v);
    this.searchChange.emit(v.trim());
  }

  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    if (!this.open()) return;
    const el = this.host.nativeElement;
    if (!el.contains(event.target as Node)) {
      this.open.set(false);
      this.onTouched();
    }
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (this.open()) this.open.set(false);
  }
}
