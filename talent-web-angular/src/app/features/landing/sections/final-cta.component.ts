import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-landing-final-cta',
  standalone: true,
  templateUrl: './final-cta.component.html',
  styleUrl: './final-cta.component.scss',
})
export class FinalCtaComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) subtitle!: string;
  @Input({ required: true }) primaryLabel!: string;
  @Input({ required: true }) secondaryLabel!: string;
  @Output() readonly navigateTo = new EventEmitter<string>();
}
