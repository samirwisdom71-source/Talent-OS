import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ExcelImportResponse, SystemApiService } from '../../services/system-api.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-settings-system-page',
  standalone: true,
  imports: [TranslatePipe, RouterLink],
  templateUrl: './settings-system-page.component.html',
  styleUrl: './settings-system-page.component.scss',
})
export class SettingsSystemPageComponent implements OnInit {
  private readonly systemApi = inject(SystemApiService);
  readonly health = signal<string | null>(null);
  readonly failed = signal(false);
  readonly loading = signal(false);
  readonly selectedTable = signal('all');
  readonly importBusy = signal(false);
  readonly importError = signal<string | null>(null);
  readonly importResult = signal<ExcelImportResponse | null>(null);

  readonly tableOptions = [
    { value: 'all', key: 'settings.import.scope.all' },
    { value: 'organization-units', key: 'settings.import.scope.organizationUnits' },
    { value: 'job-grades', key: 'settings.import.scope.jobGrades' },
    { value: 'positions', key: 'settings.import.scope.positions' },
    { value: 'competency-categories', key: 'settings.import.scope.competencyCategories' },
    { value: 'competency-levels', key: 'settings.import.scope.competencyLevels' },
    { value: 'competencies', key: 'settings.import.scope.competencies' },
    { value: 'employees', key: 'settings.import.scope.employees' },
    { value: 'roles', key: 'settings.import.scope.roles' },
    { value: 'users', key: 'settings.import.scope.users' },
    { value: 'performance-cycles', key: 'settings.import.scope.performanceCycles' },
    { value: 'critical-positions', key: 'settings.import.scope.criticalPositions' },
    { value: 'job-competency-requirements', key: 'settings.import.scope.jobCompetencyRequirements' },
    { value: 'succession-plans', key: 'settings.import.scope.successionPlans' },
    { value: 'development-plans', key: 'settings.import.scope.developmentPlans' },
    { value: 'marketplace-opportunities', key: 'settings.import.scope.marketplaceOpportunities' },
  ] as const;

  ngOnInit(): void {
    this.check();
  }

  check(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.systemApi.health().subscribe({
      next: (v) => {
        this.health.set(v);
        this.failed.set(false);
        this.loading.set(false);
      },
      error: () => {
        this.health.set(null);
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  onScopeChange(value: string): void {
    this.selectedTable.set(value || 'all');
  }

  downloadTemplate(withSampleData = false): void {
    const table = this.selectedTable();
    this.systemApi.downloadExcelTemplate(table, withSampleData).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `talent-os-import-template-${table}-${withSampleData ? 'sample' : 'blank'}.xlsx`;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.importError.set('Template download failed.');
      },
    });
  }

  onFileChosen(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.importBusy.set(true);
    this.importError.set(null);
    this.importResult.set(null);
    this.systemApi.importExcel(file, this.selectedTable()).subscribe({
      next: (result) => {
        this.importResult.set(result);
        this.importBusy.set(false);
      },
      error: () => {
        this.importError.set('Import failed.');
        this.importBusy.set(false);
      },
    });
    input.value = '';
  }
}
