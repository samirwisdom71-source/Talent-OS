import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-landing-footer',
  standalone: true,
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  @Input({ required: true }) description!: string;
  @Input({ required: true }) links!: {
    platform: string;
    modules: string;
    analytics: string;
    security: string;
    contact: string;
  };
  @Input({ required: true }) copyright!: string;
  @Output() readonly navigateTo = new EventEmitter<string>();
}
