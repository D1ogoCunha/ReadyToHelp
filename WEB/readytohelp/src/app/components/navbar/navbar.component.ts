import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  private readonly storageKey = 'rt-sidebar-open';

  isOpen = false;

  ngOnInit(): void {
    try {
      const saved = localStorage.getItem(this.storageKey);
      this.isOpen = saved ? saved === '1' : false;
    } catch { }
  }

  toggle() {
    this.isOpen = !this.isOpen;
    try {
      localStorage.setItem(this.storageKey, this.isOpen ? '1' : '0');
    } catch { }
  }

  private auth = inject(AuthService);
  
  get isAuthenticated(): boolean {
    return this.auth.isAuthenticated();
  }

  logout(): void {
    this.auth.logout(true);
  }
}
