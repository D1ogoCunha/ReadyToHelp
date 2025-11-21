import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  /**
   * Constructor for AuthInterceptor
   * @param authService Service to handle authentication
   */
  constructor(private readonly authService: AuthService) {}

  /**
   * Intercepts HTTP requests to add an Authorization header if a token is available.
   * @param request The outgoing HTTP request
   * @param next The next interceptor in the chain
   * @returns An observable of the HTTP event
   */
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    if (token) {
      const cloned = request.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
      return next.handle(cloned);
    }
    return next.handle(request);
  }
}
