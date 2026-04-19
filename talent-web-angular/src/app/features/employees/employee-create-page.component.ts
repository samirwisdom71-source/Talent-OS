import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { EmployeesApiService } from '../../services/employees-api.service';
import { CreateEmployeeRequest } from '../../shared/models/employee.models';

@Component({
  selector: 'app-employee-create-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './employee-create-page.component.html',
  styleUrl: './employee-create-page.component.scss',
})
export class EmployeeCreatePageComponent {
  private readonly api = inject(EmployeesApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  model: CreateEmployeeRequest = {
    employeeNumber: '',
    fullNameAr: '',
    fullNameEn: '',
    email: '',
    organizationUnitId: '',
    positionId: '',
  };

  save(): void {
    if (!this.model.organizationUnitId.trim() || !this.model.positionId.trim()) {
      this.toast.show('أدخل معرّف الوحدة التنظيمية والمنصب (GUID)', 'error');
      return;
    }
    this.busy.set(true);
    this.api.create(this.model).subscribe({
      next: (e) => {
        this.busy.set(false);
        this.toast.show('تم إنشاء الموظف', 'success');
        void this.router.navigate(['/employees', e.id]);
      },
      error: () => {
        this.busy.set(false);
        this.toast.show('تعذر الإنشاء — تحقق من الصلاحيات والبيانات', 'error');
      },
    });
  }
}
