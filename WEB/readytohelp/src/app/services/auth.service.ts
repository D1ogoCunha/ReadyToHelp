import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { UserService } from './user.service';

interface DecodedToken {
  email: string;
  role: string | string[];
  exp: number;
  iat: number;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly baseAuthUrl = 'https://readytohelp-api.up.railway.app/api/auth';
  private readonly tokenKey = 'authToken';

  private readonly currentUserSubject: BehaviorSubject<DecodedToken | null>;
  public currentUser: Observable<DecodedToken | null>;

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
    private readonly userService: UserService
  ) {
    let initial: DecodedToken | null = null;
    const token = localStorage.getItem(this.tokenKey);
    if (token) {
      try {
        const decoded = jwtDecode<DecodedToken>(token);
        if (decoded.exp * 1000 > Date.now()) {
          initial = decoded;
        } else {
          localStorage.removeItem(this.tokenKey);
        }
      } catch (e) {
        console.error('Error decoding token on init:', e);
        localStorage.removeItem(this.tokenKey);
      }
    }
    this.currentUserSubject = new BehaviorSubject<DecodedToken | null>(initial);
    this.currentUser = this.currentUserSubject.asObservable();
  }

  /**
   * Gets the current decoded token value.
   */
  public get currentUserValue(): DecodedToken | null {
    return this.currentUserSubject.value;
  }

  /**
   * Logs in a user with the provided credentials.
   * @param credentials An object containing email and password.
   * @returns An observable of the authentication token as a string.
   */
  login(credentials: { email: string; password: string }): Observable<string> {
    const url = `${this.baseAuthUrl}/login/web`;
    return this.http.post(url, credentials, { responseType: 'text' }).pipe(
      tap((responseText) => {
        const token =
          typeof responseText === 'string' && responseText.trim()
            ? responseText.trim()
            : null;
        if (token) this.setToken(token);
        else
          console.warn(
            'AuthService.login: token not found in response',
            responseText
          );
      })
    );
  }

  refreshToken(): Observable<string | null> {
    const token = this.getToken();
    if (!token) return of(null);
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http
      .post<string>(`${this.baseAuthUrl}/refresh-token`, {}, { headers })
      .pipe(
        map((newToken) => {
          if (newToken) this.setToken(newToken);
          return newToken || null;
        }),
        catchError(() => of(null))
      );
  }

  logout(remote = true): void {
    const token = this.getToken();
    if (remote && token) {
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      this.http.post(`${this.baseAuthUrl}/logout`, {}, { headers }).subscribe({
        next: () => {},
        error: () => {},
      });
    }

    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    try {
      this.userService.clearUser();
    } catch {}
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      return decoded.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  private setToken(token: string) {
    localStorage.setItem(this.tokenKey, token);

    try {
      const decoded = jwtDecode<any>(token);
      const userFromToken = {
        email: decoded.email ?? decoded.sub ?? null,
        role: decoded.role ?? decoded.roles ?? decoded.profile ?? null,
      };

      localStorage.setItem('user', JSON.stringify(userFromToken));
      this.userService.setUser(userFromToken);
      this.currentUserSubject.next(decoded);
    } catch (e) {
      console.error('Error decoding token on setToken:', e);
      this.currentUserSubject.next(null);
    }
  }

  register(userData: any): Observable<any> {
    return this.http.post<any>('/api/register', userData).pipe(
      catchError((err) => {
        throw err;
      })
    );
  }
}
