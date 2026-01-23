import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { LoginDto, LoginResponseDto, CreateUserDto, UserDto } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private currentUser = signal<LoginResponseDto | null>(null);
  
  isAuthenticated = computed(() => !!this.currentUser());
  user = computed(() => this.currentUser());

  constructor() {
    this.loadFromStorage();
  }

  private loadFromStorage(): void {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    if (token && userData) {
      try {
        this.currentUser.set(JSON.parse(userData));
      } catch {
        this.logout();
      }
    }
  }

  login(dto: LoginDto): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(`${environment.apiUrl}/auth/login`, dto).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response));
        this.currentUser.set(response);
      })
    );
  }

  register(dto: CreateUserDto): Observable<UserDto> {
    return this.http.post<UserDto>(`${environment.apiUrl}/users`, dto);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  checkAuth(): Observable<LoginResponseDto | null> {
    const token = this.getToken();
    if (!token) {
      return of(null);
    }
    return this.http.get<LoginResponseDto>(`${environment.apiUrl}/auth/me`).pipe(
      tap(user => this.currentUser.set(user)),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }
}
