import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service'; 

/**
 * AuthGuard
 * Prevents access to routes for unauthenticated users and redirects to login.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  /**
   * Constructor for AuthGuard
   * @param authService Service to handle authentication
   * @param router Router to navigate between routes
   */
  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  /**
   * Determines if a route can be activated based on authentication status.
   * @param route The activated route snapshot
   * @param state The router state snapshot
   * @returns True if the user is authenticated, otherwise redirects to login and returns false
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    if (this.authService.isAuthenticated()) {
      return true;
    }

    this.router.navigate(['/login/web'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}

