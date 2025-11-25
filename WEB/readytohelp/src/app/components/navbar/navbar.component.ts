import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

/**
 * NavbarComponent
 * Displays the navigation bar with links and user authentication actions.
 */
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
  private auth = inject(AuthService);

  isOpen = false;

  /**
   * OnInit lifecycle hook to initialize sidebar state from localStorage.
   */
  ngOnInit(): void {
    try {
      const saved = localStorage.getItem(this.storageKey);
      this.isOpen = saved ? saved === '1' : false;
    } catch { }
  }

  /**
   * Toggles the sidebar open/closed state and persists it to localStorage.
   */
  toggle() {
    this.isOpen = !this.isOpen;
    try {
      localStorage.setItem(this.storageKey, this.isOpen ? '1' : '0');
    } catch { }
  }
  
  /**
   * Checks if the user is authenticated.
   * @returns True if authenticated, false otherwise.
   */
  get isAuthenticated(): boolean {
    return this.auth.isAuthenticated();
  }

  /**   
   * Logs out the current user.
   */
  logout(): void {
    this.auth.logout(true);
  }
}
