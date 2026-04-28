import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-landing-hero',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss',
})
export class HeroComponent {
  @Input({ required: true }) lang!: 'en' | 'ar';
  @Input({ required: true }) hero!: {
    title: string;
    subtitle: string;
    primary: string;
    secondary: string;
  };
  @Output() readonly navigateTo = new EventEmitter<string>();
}
