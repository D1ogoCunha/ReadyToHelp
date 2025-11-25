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

  private apiUrl = 'https://readytohelp-api.up.railway.app/api/user';

  constructor(private http: HttpClient) {}

  /**
   * Sets the current user and updates local storage.
   * @param user The user object to set.
   */
  setUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

  /**
   * Clears the current user and removes it from local storage.
   */
  clearUser() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
  }

  /**
   * Create a new user (ADMIN/MANAGER)
   * @param payload The user data to create.
   * @returns An observable of the created User object.
   */
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

  /**
   * Update an existing user
   * @param id The ID of the user to update.
   * @param payload The user data to update.
   * @returns An observable of the updated User object.
   */
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

  /**
   * Delete a user by ID
   * @param id The ID of the user to delete.
   * @returns An observable of the deleted User object.
   */
  deleteUser(id: number): Observable<User> {
    return this.http.delete<User>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get all users with pagination, sorting, and filtering
   * @param pageNumber The page number (default is 1).
   * @param pageSize The number of items per page (default is 10).
   * @param sortBy The field to sort by (default is 'Name').
   * @param sortOrder The sort order ('asc' or 'desc', default is 'asc').
   * @param filter A filter string to search users (default is '').
   * @returns An observable of an array of User objects.
   */
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
