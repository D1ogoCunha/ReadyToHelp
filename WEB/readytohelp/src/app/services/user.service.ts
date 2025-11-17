import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly userSubject = new BehaviorSubject<any>(JSON.parse(localStorage.getItem('user') || '{}'));
  user$ = this.userSubject.asObservable();

  setUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

   clearUser() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
  }
}
