import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-landing-outcomes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './outcomes.component.html',
  styleUrl: './outcomes.component.scss',
})
export class OutcomesComponent {
  @Input({ required: true }) eyebrow!: string;
  @Input({ required: true }) title!: string;
  @Input({ required: true }) lead!: string;
  @Input({ required: true }) items!: string[];
}
