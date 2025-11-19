import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterModule } from '@angular/router'; 

@Component({
  selector: 'app-navbar',
  standalone: true, 
  imports: [
    RouterModule
  ], 
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  isOpen = true;
  toggle() { this.isOpen = !this.isOpen; }
}