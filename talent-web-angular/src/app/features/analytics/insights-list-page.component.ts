import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/auth/auth.service';
import { IntelligenceApiService } from '../../services/intelligence-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { PagedResult } from '../../shared/models/api.types';
import { TalentInsightDto } from '../../shared/models/intelligence.models';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { IdChipComponent } from '../../shared/ui/id-chip.component';

const INSIGHT_ACTIVE = 1;

@Component({
  selector: 'app-insights-list-page',
  standalone: true,
  imports: [DatePipe, IdChipComponent],
  templateUrl: './insights-list-page.component.html',
  styleUrl: './insights-list-page.component.scss',
})
export class InsightsListPageComponent implements OnInit {
  private readonly api = inject(IntelligenceApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<TalentInsightDto> | null>(null);
  readonly failed = signal(false);
  private readonly busy = signal<Record<string, boolean>>({});

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getInsightsPaged({ page: 1, pageSize: 50 }).subscribe({
      next: (d) => {
        this.data.set(d);
        this.failed.set(false);
      },
      error: () => {
        this.data.set(null);
        this.failed.set(true);
      },
    });
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  statusLabel(v: number): string {
    return EnumLabels.insightStatus(this.lang(), v);
  }

  isRowBusy(id: string): boolean {
    return this.busy()[id] ?? false;
  }

  dismiss(row: TalentInsightDto): void {
    if (row.status !== INSIGHT_ACTIVE) return;
    this.setBusy(row.id, true);
    this.api.dismissInsight(row.id).subscribe({
      next: () => {
        this.setBusy(row.id, false);
        this.toast.show('تم تجاهل الرؤية', 'success');
        this.load();
      },
      error: () => {
        this.setBusy(row.id, false);
        this.toast.show('تعذر التجاهل', 'error');
      },
    });
  }

  private setBusy(id: string, v: boolean): void {
    this.busy.update((m) => ({ ...m, [id]: v }));
  }
}
