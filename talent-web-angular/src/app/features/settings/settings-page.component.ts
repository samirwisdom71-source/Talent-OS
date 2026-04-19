import { Component, inject } from '@angular/core';
import { I18nService } from '../../shared/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss',
})
export class SettingsPageComponent {
  readonly i18n = inject(I18nService);
}
