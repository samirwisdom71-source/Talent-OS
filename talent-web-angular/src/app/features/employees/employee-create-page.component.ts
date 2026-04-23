import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { EmployeesApiService } from '../../services/employees-api.service';
import { I18nService } from '../../shared/services/i18n.service';
import { CreateEmployeeRequest } from '../../shared/models/employee.models';
import { LookupSearchComboComponent } from '../../shared/ui/lookup-search-combo.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-employee-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule, LookupSearchComboComponent, TranslatePipe],
  templateUrl: './employee-create-page.component.html',
  styleUrl: './employee-create-page.component.scss',
})
export class EmployeeCreatePageComponent {
  private readonly api = inject(EmployeesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly busy = signal(false);

  readonly organizationUnitId = signal<string>('');
  readonly positionId = signal<string>('');
  readonly positionFilter = computed(() => this.organizationUnitId() || null);

  model: Omit<CreateEmployeeRequest, 'organizationUnitId' | 'positionId'> = {
    employeeNumber: '',
    fullNameAr: '',
    fullNameEn: '',
    email: '',
  };

  onOrganizationUnitChange(id: string): void {
    this.organizationUnitId.set(id);
    this.positionId.set('');
  }

  save(): void {
    const ou = this.organizationUnitId().trim();
    const pos = this.positionId().trim();
    if (!ou || !pos) {
      this.toast.show(this.i18n.t('employeeCreate.toast.requiredUnitPosition'), 'error');
      return;
    }
    if (!this.model.employeeNumber.trim() || !this.model.fullNameAr.trim() || !this.model.email.trim()) {
      this.toast.show(this.i18n.t('employeeCreate.toast.requiredFields'), 'error');
      return;
    }

    this.busy.set(true);
    const body: CreateEmployeeRequest = {
      ...this.model,
      employeeNumber: this.model.employeeNumber.trim(),
      fullNameAr: this.model.fullNameAr.trim(),
      fullNameEn: this.model.fullNameEn.trim(),
      email: this.model.email.trim(),
      organizationUnitId: ou,
      positionId: pos,
    };
    this.api.create(body).subscribe({
      next: (e) => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('employeeCreate.toast.created'), 'success');
        void this.router.navigate(['/employees', e.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show(this.i18n.t('employeeCreate.toast.createFailed'), 'error');
      },
    });
  }
}
