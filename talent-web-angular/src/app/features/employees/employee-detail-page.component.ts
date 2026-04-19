import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EmployeesApiService } from '../../services/employees-api.service';
import { EmployeeDto } from '../../shared/models/employee.models';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-employee-detail-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './employee-detail-page.component.html',
  styleUrl: './employee-detail-page.component.scss',
})
export class EmployeeDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(EmployeesApiService);
  readonly i18n = inject(I18nService);

  readonly employee = signal<EmployeeDto | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.api.getById(id).subscribe({
      next: (e) => {
        this.employee.set(e);
        this.failed.set(false);
      },
      error: () => {
        this.employee.set(null);
        this.failed.set(true);
      },
    });
  }

  displayName(e: EmployeeDto): string {
    return this.i18n.lang() === 'ar' ? e.fullNameAr || e.fullNameEn : e.fullNameEn || e.fullNameAr;
  }
}
