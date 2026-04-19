import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { NotificationTemplatesApiService } from '../../services/notification-templates-api.service';
import { PagedResult } from '../../shared/models/api.types';
import {
  CreateNotificationTemplateRequest,
  NotificationTemplateDto,
  UpdateNotificationTemplateRequest,
} from '../../shared/models/notification.models';
import { PermissionCodes } from '../../shared/models/permission-codes';
import { I18nService } from '../../shared/services/i18n.service';
import { IdChipComponent } from '../../shared/ui/id-chip.component';
import { EnumLabels, UiLang } from '../../shared/utils/enum-labels';

@Component({
  selector: 'app-notification-templates-page',
  standalone: true,
  imports: [FormsModule, IdChipComponent, RouterLink],
  templateUrl: './notification-templates-page.component.html',
  styleUrl: './notification-templates-page.component.scss',
})
export class NotificationTemplatesPageComponent implements OnInit {
  private readonly api = inject(NotificationTemplatesApiService);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly i18n = inject(I18nService);

  readonly PermissionCodes = PermissionCodes;

  readonly data = signal<PagedResult<NotificationTemplateDto> | null>(null);
  readonly failed = signal(false);
  readonly busy = signal(false);

  readonly editorOpen = signal(false);
  readonly editingId = signal<string | null>(null);

  draftCreate: CreateNotificationTemplateRequest = {
    code: '',
    name: '',
    subjectTemplate: '',
    bodyTemplate: '',
    notificationType: 99,
    channel: 1,
  };

  draftUpdate: UpdateNotificationTemplateRequest = {
    name: '',
    subjectTemplate: '',
    bodyTemplate: '',
    notificationType: 99,
    channel: 1,
    isActive: true,
  };

  ngOnInit(): void {
    this.load();
  }

  lang(): UiLang {
    return this.i18n.lang();
  }

  channelLabel(v: number): string {
    return EnumLabels.notificationChannel(this.lang(), v);
  }

  typeLabel(v: number): string {
    return EnumLabels.notificationType(this.lang(), v);
  }

  load(): void {
    this.failed.set(false);
    this.api.getPaged({ page: 1, pageSize: 100, activeOnly: null, search: null }).subscribe({
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

  openCreate(): void {
    this.editingId.set(null);
    this.draftCreate = {
      code: '',
      name: '',
      subjectTemplate: '',
      bodyTemplate: '',
      notificationType: 99,
      channel: 1,
    };
    this.editorOpen.set(true);
  }

  openEdit(row: NotificationTemplateDto): void {
    this.editingId.set(row.id);
    this.draftUpdate = {
      name: row.name,
      subjectTemplate: row.subjectTemplate ?? '',
      bodyTemplate: row.bodyTemplate,
      notificationType: row.notificationType,
      channel: row.channel,
      isActive: row.isActive,
    };
    this.editorOpen.set(true);
  }

  closeEditor(): void {
    this.editorOpen.set(false);
    this.editingId.set(null);
  }

  save(): void {
    const id = this.editingId();
    if (id) {
      this.busy.set(true);
      this.api.update(id, this.draftUpdate).subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.show('تم الحفظ', 'success');
          this.closeEditor();
          this.load();
        },
        error: () => {
          this.busy.set(false);
          this.toast.show('تعذر الحفظ', 'error');
        },
      });
      return;
    }
    if (!this.draftCreate.code.trim() || !this.draftCreate.name.trim() || !this.draftCreate.bodyTemplate.trim()) {
      this.toast.show('أكمل الحقول المطلوبة', 'error');
      return;
    }
    this.busy.set(true);
    this.api
      .create({
        ...this.draftCreate,
        code: this.draftCreate.code.trim(),
        name: this.draftCreate.name.trim(),
        subjectTemplate: this.draftCreate.subjectTemplate?.trim() || null,
        bodyTemplate: this.draftCreate.bodyTemplate.trim(),
      })
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.show('تم الإنشاء', 'success');
          this.closeEditor();
          this.load();
        },
        error: () => {
          this.busy.set(false);
          this.toast.show('تعذر الإنشاء', 'error');
        },
      });
  }
}
