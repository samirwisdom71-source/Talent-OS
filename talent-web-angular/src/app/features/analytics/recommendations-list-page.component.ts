import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/auth/auth.service';
import { IntelligenceApiService } from '../../services/intelligence-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { PagedResult } from '../../shared/models/api.types';
import { TalentRecommendationDto } from '../../shared/models/intelligence.models';
import { I18nService } from '../../shared/services/i18n.service';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';
import { IdChipComponent } from '../../shared/ui/id-chip.component';

const REC_ACTIVE = 1;

@Component({
  selector: 'app-recommendations-list-page',
  standalone: true,
  imports: [DatePipe, IdChipComponent],
  templateUrl: './recommendations-list-page.component.html',
  styleUrl: './recommendations-list-page.component.scss',
})
export class RecommendationsListPageComponent implements OnInit {
  private readonly api = inject(IntelligenceApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<TalentRecommendationDto> | null>(null);
  readonly failed = signal(false);
  private readonly busy = signal<Record<string, boolean>>({});

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getRecommendationsPaged({ page: 1, pageSize: 50 }).subscribe({
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
    return EnumLabels.recommendationStatus(this.lang(), v);
  }

  isRowBusy(id: string): boolean {
    return this.busy()[id] ?? false;
  }

  dismiss(row: TalentRecommendationDto): void {
    if (row.status !== REC_ACTIVE) return;
    this.setBusy(row.id, true);
    this.api.dismissRecommendation(row.id).subscribe({
      next: () => {
        this.setBusy(row.id, false);
        this.toast.show('تم تجاهل التوصية', 'success');
        this.load();
      },
      error: () => {
        this.setBusy(row.id, false);
        this.toast.show('تعذر التجاهل', 'error');
      },
    });
  }

  accept(row: TalentRecommendationDto): void {
    if (row.status !== REC_ACTIVE) return;
    this.setBusy(row.id, true);
    this.api.acceptRecommendation(row.id).subscribe({
      next: () => {
        this.setBusy(row.id, false);
        this.toast.show('تم قبول التوصية', 'success');
        this.load();
      },
      error: () => {
        this.setBusy(row.id, false);
        this.toast.show('تعذر القبول', 'error');
      },
    });
  }

  private setBusy(id: string, v: boolean): void {
    this.busy.update((m) => ({ ...m, [id]: v }));
  }
}
