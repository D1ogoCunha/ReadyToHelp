import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

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
  toast: { show: boolean; message: string; type: 'success' | 'error' } = {
    show: false,
    message: '',
    type: 'success',
  };

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {
    const q = this.route.snapshot.queryParams['returnUrl'];
    if (q) this.returnUrl = q;
  }

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

  showToast(message: string, type: 'success' | 'error') {
    this.toast = { show: true, message, type };
    setTimeout(() => (this.toast.show = false), 3000);
  }
}
