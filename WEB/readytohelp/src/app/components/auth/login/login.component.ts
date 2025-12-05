import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

/**
 * LoginComponent
 * Handles user authentication and login form logic.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  returnUrl = '/';
  /**
   * Toast notification object
   * @property show - Whether the toast is visible
   * @property message - Message to display
   * @property type - Type of message ('success' | 'error')
   */
  toast: { show: boolean; message: string; type: 'success' | 'error' } = {
    show: false,
    message: '',
    type: 'success',
  };

  /**
   * Injects authentication, routing, and route services.
   * Sets the return URL from query parameters if provided.
   * @param auth AuthService for authentication logic
   * @param router Router for navigation
   * @param route ActivatedRoute for accessing route parameters
   */
  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {
    const q = this.route.snapshot.queryParams['returnUrl'];
    if (q) this.returnUrl = q;
  }

  /**
   * Handles form submission for login.
   * Calls AuthService and manages loading state and error handling.
   */
  submit() {
    this.loading = true;
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.loading = false;
        let errorMsg = '';

        // Error handling for different HTTP status codes and error formats
        if (err?.status === 401) {
          errorMsg = 'Login failed. Please check your credentials.';
        } else if (err?.status === 403) {
          errorMsg =
            'Access denied: account does not have permission to log in.';
        } else if (err?.error) {
          if (typeof err.error === 'string') {
            errorMsg = `Error ${err.status}: ${err.statusText || err.error}`;
          } else if (err.error?.message) {
            errorMsg = err.error.message;
          } else {
            errorMsg = JSON.stringify(err.error);
          }
        } else {
          errorMsg = err?.message || 'An unknown error occurred during login.';
        }

        this.showToast(errorMsg, 'error');
      },
    });
  }

  /**
   * Displays a toast notification for 3 seconds.
   * @param message Message to display
   * @param type Type of message ('success' | 'error')
   */
  showToast(message: string, type: 'success' | 'error') {
    this.toast = { show: true, message, type };
    setTimeout(() => (this.toast.show = false), 3000);
  }
}