import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-landing-value-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './value-strip.component.html',
  styleUrl: './value-strip.component.scss',
})
export class ValueStripComponent {
  @Input({ required: true }) tags!: string[];
}
