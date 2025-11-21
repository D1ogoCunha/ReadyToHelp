import { Component } from '@angular/core';

@Component({
  selector: 'app-legal-footer',
  standalone: true,
  templateUrl: './legal-footer.component.html',
  styleUrls: ['./legal-footer.component.css'],
})
export class LegalFooterComponent {
  readonly year = new Date().getFullYear();
}