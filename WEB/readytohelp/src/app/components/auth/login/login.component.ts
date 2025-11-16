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
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error: string | null = null;
  returnUrl = '/';

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {
    const q = this.route.snapshot.queryParams['returnUrl'];
    if (q) this.returnUrl = q;
  }

  submit() {
    this.error = null;
    this.loading = true;
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.loading = false;

        if (err?.status === 401) {
          this.error = 'Login failed. Please check your credentials.';
          return;
        }
        if (err?.status === 403) {
          this.error = 'Access denied: account does not have permission to log in.';
          return;
  }

        if (err?.error) {
          if (typeof err.error === 'string') {
            this.error = `Error ${err.status}: ${err.statusText || err.error}`;
          } else if (err.error?.message) {
            this.error = err.error.message;
          } else {
            this.error = JSON.stringify(err.error);
          }
        } else {
          this.error = err?.message || 'An unknown error occurred during login.';
        }
      }
    });
  }
}