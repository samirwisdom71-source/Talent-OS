import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { PermissionCodes } from '../../shared/models/permission-codes';

@Component({
  selector: 'app-approvals-hub',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, TranslatePipe],
  templateUrl: './approvals-hub.component.html',
  styleUrl: './approvals-hub.component.scss',
})
export class ApprovalsHubComponent {
  readonly auth = inject(AuthService);
  readonly PermissionCodes = PermissionCodes;
}
