import { Component } from '@angular/core';

/**
 * LegalFooterComponent
 * Displays legal and copyright information in the footer section.
 */
@Component({
  selector: 'app-legal-footer',
  standalone: true,
  templateUrl: './legal-footer.component.html',
  styleUrls: ['./legal-footer.component.css'],
})
export class LegalFooterComponent {
  /** Current year for copyright notice */
  readonly year = new Date().getFullYear();
}