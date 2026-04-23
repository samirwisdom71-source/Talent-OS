import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { ClassificationRuleSetsApiService } from '../../services/classification-rule-sets-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  ClassificationRuleSetDto,
  CreateClassificationRuleSetRequest,
  UpdateClassificationRuleSetRequest,
} from '../../shared/models/classification-rule-set.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-classification-rule-sets-page',
  standalone: true,
  imports: [TranslatePipe, FormsModule, DatePipe, RouterLink],
  templateUrl: './classification-rule-sets-page.component.html',
  styleUrl: './classification-rule-sets-page.component.scss',
})
export class ClassificationRuleSetsPageComponent implements OnInit {
  private readonly rulesApi = inject(ClassificationRuleSetsApiService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly viewMode = signal<ViewMode>('table');
  readonly loading = signal(false);
  readonly rules = signal<PagedResult<ClassificationRuleSetDto> | null>(null);
  readonly rulesFailed = signal(false);
  readonly saving = signal(false);
  readonly editId = signal<string | null>(null);
  readonly modalOpen = signal(false);

  readonly formTitle = computed(() =>
    this.editId() ? this.i18n.t('settings.rules.edit') : this.i18n.t('settings.rules.create'),
  );

  readonly form = signal<CreateClassificationRuleSetRequest>({
    name: '',
    version: '',
    lowThreshold: 0,
    highThreshold: 0,
    effectiveFromUtc: new Date().toISOString().slice(0, 10),
    notes: null,
  });

  ngOnInit(): void {
    this.loadRules();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  loadRules(): void {
    this.loading.set(true);
    this.rulesFailed.set(false);
    this.rulesApi.getPaged({ page: 1, pageSize: 100 }).subscribe({
      next: (v) => {
        this.rules.set(v);
        this.rulesFailed.set(false);
        this.loading.set(false);
      },
      error: () => {
        this.rules.set(null);
        this.rulesFailed.set(true);
        this.loading.set(false);
      },
    });
  }

  edit(row: ClassificationRuleSetDto): void {
    this.editId.set(row.id);
    this.form.set({
      name: row.name,
      version: row.version,
      lowThreshold: row.lowThreshold,
      highThreshold: row.highThreshold,
      effectiveFromUtc: row.effectiveFromUtc.slice(0, 10),
      notes: row.notes ?? null,
    });
    this.modalOpen.set(true);
  }

  openCreateModal(): void {
    this.resetForm();
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    this.modalOpen.set(false);
  }

  resetForm(): void {
    this.editId.set(null);
    this.form.set({
      name: '',
      version: '',
      lowThreshold: 0,
      highThreshold: 0,
      effectiveFromUtc: new Date().toISOString().slice(0, 10),
      notes: null,
    });
  }

  saveForm(): void {
    const payload = this.form();
    if (!payload.name.trim() || !payload.version.trim()) return;
    this.saving.set(true);
    const body: CreateClassificationRuleSetRequest = {
      ...payload,
      name: payload.name.trim(),
      version: payload.version.trim(),
      notes: payload.notes?.trim() || null,
    };
    const id = this.editId();
    const req$ = id ? this.rulesApi.update(id, body as UpdateClassificationRuleSetRequest) : this.rulesApi.create(body);
    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.show(this.i18n.t(id ? 'settings.toast.updated' : 'settings.toast.created'), 'success');
        this.resetForm();
        this.modalOpen.set(false);
        this.loadRules();
      },
      error: () => {
        this.saving.set(false);
        this.toast.show(this.i18n.t('settings.toast.saveFailed'), 'error');
      },
    });
  }

  activate(id: string): void {
    this.rulesApi.activate(id).subscribe({
      next: () => {
        this.toast.show(this.i18n.t('settings.toast.activated'), 'success');
        this.loadRules();
      },
      error: () => this.toast.show(this.i18n.t('settings.toast.activateFailed'), 'error'),
    });
  }

  recordStatusLabel(v: number): string {
    return EnumLabels.recordStatus(this.lang(), v);
  }

  updateFormText(field: 'name' | 'version' | 'effectiveFromUtc' | 'notes', value: string): void {
    const prev = this.form();
    this.form.set({ ...prev, [field]: value });
  }

  updateFormNumber(field: 'lowThreshold' | 'highThreshold', value: string | number): void {
    const prev = this.form();
    this.form.set({ ...prev, [field]: Number(value) || 0 });
  }
}
