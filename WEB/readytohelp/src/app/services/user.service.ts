import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly userSubject = new BehaviorSubject<any>(
    JSON.parse(localStorage.getItem('user') || '{}')
  );
  user$ = this.userSubject.asObservable();

  private apiUrl = 'http://localhost:5134/api/user';

  constructor(private http: HttpClient) {}

  setUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

  clearUser() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
  }

  // Create user (ADMIN/MANAGER)
  createUser(payload: {
    name: string;
    email: string;
    password: string;
    profile: 'CITIZEN' | 'MANAGER' | 'ADMIN';
  }): Observable<User> {
    return this.http.post<User>(this.apiUrl, {
      Name: payload.name,
      Email: payload.email,
      Password: payload.password,
      Profile: payload.profile,
    });
  }

  // Update user (ADMIN/MANAGER)
  updateUser(
    id: number,
    payload: {
      name: string;
      email: string;
      password?: string;
      profile: 'CITIZEN' | 'MANAGER' | 'ADMIN';
    }
  ): Observable<User> {
    // Sends only the fields that can be updated
    const body: any = {
      Name: payload.name,
      Email: payload.email,
      Profile: payload.profile,
    };
    if (payload.password && payload.password.trim()) {
      body.Password = payload.password;
    }
    return this.http.put<User>(`${this.apiUrl}/${id}`, body);
  }

  deleteUser(id: number): Observable<User> {
    return this.http.delete<User>(`${this.apiUrl}/${id}`);
  }

  getAllUsers(
    pageNumber = 1,
    pageSize = 10,
    sortBy = 'Name',
    sortOrder = 'asc',
    filter = ''
  ): Observable<User[]> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder)
      .set('filter', filter);
    return this.http.get<User[]>(this.apiUrl, { params });
  }
}
