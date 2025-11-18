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

  private apiUrl = 'https://readytohelp-api.azurewebsites.net/api/user';

  constructor(private http: HttpClient) {}

  setUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

  clearUser() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
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
