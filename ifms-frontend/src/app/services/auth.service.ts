import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly base = environment.authApiUrl;

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/login`, { email, password }).pipe(tap((r) => this.persistAuth(r)));
  }

  requestLoginOtp(identifier: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/login/otp/request`, { identifier });
  }

  verifyLoginOtp(identifier: string, code: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/login/otp/verify`, { identifier, code }).pipe(tap((r) => this.persistAuth(r)));
  }

  requestPasswordResetOtp(identifier: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/password/reset/request`, { identifier });
  }

  resetPassword(identifier: string, code: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/password/reset/confirm`, {
      identifier,
      code,
      newPassword
    });
  }

  register(fullName: string, email: string, password: string, role: string, phoneNumber?: string | null): Observable<{ message: string }> {
    const body: Record<string, string> = { fullName, email, password, role };
    if (phoneNumber?.trim()) body['phoneNumber'] = phoneNumber.trim();
    return this.http.post<{ message: string }>(`${this.base}/register`, body);
  }

  logout(): void {
    this.safeRemoveItem('token');
    this.safeRemoveItem('role');
    this.safeRemoveItem('fullName');
  }

  getToken(): string | null {
    return this.safeGetItem('token');
  }

  getRole(): string | null {
    return this.safeGetItem('role');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  /** JWT `sub` / NameIdentifier — required for booking API customer id. */
  getCustomerIdFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;
      const json = JSON.parse(
        globalThis.atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
      ) as Record<string, unknown>;
      const nameIdKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
      const sub = json['sub'];
      const nameId = json[nameIdKey];
      if (typeof sub === 'string' && sub.length > 0) return sub;
      if (typeof nameId === 'string' && nameId.length > 0) return nameId;
      return null;
    } catch {
      return null;
    }
  }

  getFullName(): string | null {
    return this.safeGetItem('fullName');
  }

  private persistAuth(response: AuthResponse): void {
    this.safeSetItem('token', response.token);
    this.safeSetItem('role', response.role);
    this.safeSetItem('fullName', response.fullName);
  }

  private safeGetItem(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private safeSetItem(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      /* noop */
    }
  }

  private safeRemoveItem(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      /* noop */
    }
  }
}

interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  role: string;
}
