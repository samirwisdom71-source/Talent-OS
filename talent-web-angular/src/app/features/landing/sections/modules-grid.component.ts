import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-landing-modules-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modules-grid.component.html',
  styleUrl: './modules-grid.component.scss',
})
export class ModulesGridComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) lead!: string;
  @Input({ required: true }) cards!: Array<{ icon: string; title: string; description: string }>;
}
