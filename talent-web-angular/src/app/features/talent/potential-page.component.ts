import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { PotentialAssessmentsApiService } from '../../services/potential-assessments-api.service';
import { PagedResult } from '../../shared/models/api.types';
import { PotentialAssessmentDto } from '../../shared/models/potential.models';
import { I18nService } from '../../shared/services/i18n.service';
import { IdChipComponent } from '../../shared/ui/id-chip.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-potential-page',
  standalone: true,
  imports: [DecimalPipe, IdChipComponent],
  templateUrl: './potential-page.component.html',
  styleUrl: './talent-pages.component.scss',
})
export class PotentialPageComponent implements OnInit {
  private readonly api = inject(PotentialAssessmentsApiService);
  readonly i18n = inject(I18nService);

  readonly data = signal<PagedResult<PotentialAssessmentDto> | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.api.getPaged({ page: 1, pageSize: 50 }).subscribe({
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

  potentialLevel(v: number): string {
    return EnumLabels.potentialLevel(this.lang(), v);
  }

  assessmentStatus(v: number): string {
    return EnumLabels.potentialAssessmentStatus(this.lang(), v);
  }
}
