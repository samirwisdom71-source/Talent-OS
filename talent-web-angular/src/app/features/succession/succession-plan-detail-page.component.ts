import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { IdentityLookupsApiService } from '../../services/identity-lookups-api.service';
import { PerformanceCyclesApiService } from '../../services/performance-cycles-api.service';
import { SuccessionApiService } from '../../services/succession-api.service';
import { SuccessorCandidatesApiService } from '../../services/successor-candidates-api.service';
import { PermissionCodes } from '../../shared/models/permission-codes';
import {
  CreateSuccessorCandidateRequest,
  SuccessorCandidateDto,
} from '../../shared/models/successor-candidate.models';
import { SuccessionPlanDto } from '../../shared/models/succession.models';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { I18nService } from '../../shared/services/i18n.service';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-succession-plan-detail-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, FormsModule, LookupSearchComboComponent, TranslatePipe],
  templateUrl: './succession-plan-detail-page.component.html',
  styleUrl: './succession-plan-detail-page.component.scss',
})
export class SuccessionPlanDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(SuccessionApiService);
  private readonly candidatesApi = inject(SuccessorCandidatesApiService);
  private readonly identityLookups = inject(IdentityLookupsApiService);
  private readonly cyclesApi = inject(PerformanceCyclesApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly plan = signal<SuccessionPlanDto | null>(null);
  readonly candidates = signal<readonly SuccessorCandidateDto[]>([]);
  readonly failed = signal(false);
  readonly busy = signal(false);
  readonly busyRowId = signal<string | null>(null);

  readonly showAddModal = signal(false);

  private readonly employeeNames = signal(new Map<string, string>());
  private readonly criticalLabels = signal(new Map<string, string>());
  private readonly cycleLabels = signal(new Map<string, string>());

  addForm: CreateSuccessorCandidateRequest = {
    successionPlanId: '',
    employeeId: '',
    readinessLevel: 1,
    rankOrder: 1,
    isPrimarySuccessor: false,
    notes: '',
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reload(id);
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  planStatus(s: number): string {
    return EnumLabels.successionPlanStatus(this.lang(), s);
  }

  readiness(v: number): string {
    return EnumLabels.readinessLevel(this.lang(), v);
  }

  sortedCandidates(): readonly SuccessorCandidateDto[] {
    return [...this.candidates()].sort((a, b) => a.rankOrder - b.rankOrder);
  }

  employeeName(id: string): string {
    return this.employeeNames().get(id) ?? '';
  }

  criticalLabel(id: string): string {
    return this.criticalLabels().get(id) ?? '';
  }

  cycleLabel(id: string): string {
    return this.cycleLabels().get(id) ?? '';
  }

  reload(planId: string): void {
    this.failed.set(false);
    const lang = this.i18n.lang() as 'ar' | 'en';
    forkJoin({
      plan: this.api.getPlanById(planId),
      cand: this.candidatesApi.getPaged({ page: 1, pageSize: 200, successionPlanId: planId }),
      cpLookup: this.api.getCriticalPositionsLookup({ take: 400, activeOnly: true, lang }),
      cylLookup: this.cyclesApi.getLookup({ take: 250, lang }),
      empLookup: this.identityLookups.getEmployees(undefined, 450),
    }).subscribe({
      next: ({ plan, cand, cpLookup, cylLookup, empLookup }) => {
        this.plan.set(plan);
        this.candidates.set(cand.items);
        this.criticalLabels.set(new Map(cpLookup.map((x) => [x.id, x.name])));
        this.cycleLabels.set(new Map(cylLookup.map((x) => [x.id, x.name])));
        this.employeeNames.set(new Map(empLookup.map((x) => [x.id, x.name])));
        this.addForm = {
          ...this.addForm,
          successionPlanId: planId,
        };
        this.failed.set(false);
      },
      error: () => {
        this.plan.set(null);
        this.candidates.set([]);
        this.failed.set(true);
      },
    });
  }

  openAdd(): void {
    const p = this.plan();
    if (!p) return;
    this.addForm = {
      successionPlanId: p.id,
      employeeId: '',
      readinessLevel: 2,
      rankOrder: (this.candidates().length || 0) + 1,
      isPrimarySuccessor: false,
      notes: '',
    };
    this.showAddModal.set(true);
  }

  closeAdd(): void {
    this.showAddModal.set(false);
  }

  saveCandidate(): void {
    if (!this.addForm.employeeId.trim()) {
      this.toast.show(this.i18n.t('اختر الموظف من القائمة'), 'error');
      return;
    }
    this.busy.set(true);
    this.candidatesApi
      .create({
        successionPlanId: this.addForm.successionPlanId,
        employeeId: this.addForm.employeeId.trim(),
        readinessLevel: Number(this.addForm.readinessLevel),
        rankOrder: Number(this.addForm.rankOrder),
        isPrimarySuccessor: !!this.addForm.isPrimarySuccessor,
        notes: this.addForm.notes?.trim() || null,
      })
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.closeAdd();
          this.toast.show(this.i18n.t('تمت إضافة المرشح'), 'success');
          this.reload(this.addForm.successionPlanId);
        },
        error: () => {
          this.busy.set(false);
          this.toast.show(this.i18n.t('تعذرت الإضافة'), 'error');
        },
      });
  }

  markPrimary(c: SuccessorCandidateDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.busyRowId.set(c.id);
    this.candidatesApi.markPrimary(c.id).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تم تعيين الخليفة الأساسي'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تعذر التعيين'), 'error');
      },
    });
  }

  removeCandidate(c: SuccessorCandidateDto): void {
    const pid = this.plan()?.id;
    if (!pid) return;
    this.busyRowId.set(c.id);
    this.candidatesApi.remove(c.id).subscribe({
      next: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تم الحذف'), 'success');
        this.reload(pid);
      },
      error: () => {
        this.busyRowId.set(null);
        this.toast.show(this.i18n.t('تعذر الحذف'), 'error');
      },
    });
  }
}
