import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { EmployeesApiService } from '../../services/employees-api.service';
import { EmployeeListItemDto } from '../../shared/models/employee.models';
import { PagedResult } from '../../shared/models/api.types';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';

@Component({
  selector: 'app-employees-list-page',
  standalone: true,
  imports: [RouterLink, FormsModule, TranslatePipe],
  templateUrl: './employees-list-page.component.html',
  styleUrl: './employees-list-page.component.scss',
})
export class EmployeesListPageComponent implements OnInit {
  private readonly api = inject(EmployeesApiService);
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18nService);
  readonly PermissionCodes = PermissionCodes;

  search = '';
  page = 1;
  readonly pageSize = 25;

  readonly result = signal<PagedResult<EmployeeListItemDto> | null>(null);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.failed.set(false);
    this.api.getPaged({ page: this.page, pageSize: this.pageSize, search: this.search || null }).subscribe({
      next: (r) => this.result.set(r),
      error: () => {
        this.result.set(null);
        this.failed.set(true);
      },
    });
  }

  nextPage(): void {
    const r = this.result();
    if (!r || !r.hasNextPage) return;
    this.page += 1;
    this.load();
  }

  prevPage(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }

  displayName(row: EmployeeListItemDto): string {
    return this.i18n.lang() === 'ar' ? row.fullNameAr || row.fullNameEn : row.fullNameEn || row.fullNameAr;
  }
}
