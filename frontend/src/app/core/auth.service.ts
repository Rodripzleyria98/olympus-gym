import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { API_BASE_URL } from './api.config';

export interface UsuarioSesion {
  id: string;
  nombre: string;
  apellido: string;
  email: string;
  rol: 'Admin' | 'Profesor' | 'User' | 'Socio';
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  usuario: UsuarioSesion;
}

const SESSION_KEY = 'olympus.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly session = signal<AuthResponse | null>(this.restoreSession());

  readonly currentUser = computed(() => this.session()?.usuario ?? null);
  readonly token = computed(() => this.session()?.token ?? null);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.hasRole('Admin'));
  readonly isProfesor = computed(() => this.hasRole('Profesor'));

  getRol(): string | null {
    return this.getRoles()[0] ?? null;
  }

  hasRole(role: string): boolean {
    return this.getRoles().some((value) => value.toLowerCase() === role.toLowerCase());
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  private getRoles(): string[] {
    const token = this.token();
    if (!token) return [];
    try {
      const payloadSegment = token.split('.')[1];
      if (!payloadSegment) return [];
      const base64 = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
      const payload = JSON.parse(atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='))) as Record<string, unknown>;
      const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? payload['role'];
      if (typeof role === 'string') return [role];
      return Array.isArray(role) ? role.filter((value): value is string => typeof value === 'string') : [];
    } catch {
      return [];
    }
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/login`, { email, password }).pipe(
      tap((response) => this.saveSession(response)),
    );
  }

  register(payload: { nombre: string; apellido: string; dni: string; email: string; password: string }) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/register`, payload).pipe(
      tap((response) => this.saveSession(response)),
    );
  }

  logout(): void {
    sessionStorage.removeItem(SESSION_KEY);
    this.session.set(null);
    void this.router.navigateByUrl('/login');
  }

  private saveSession(response: AuthResponse): void {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(response));
    this.session.set(response);
  }

  private restoreSession(): AuthResponse | null {
    try {
      const saved = sessionStorage.getItem(SESSION_KEY);
      if (!saved) return null;
      const session = JSON.parse(saved) as AuthResponse;
      if (new Date(session.expiresAt).getTime() <= Date.now()) {
        sessionStorage.removeItem(SESSION_KEY);
        return null;
      }
      return session;
    } catch {
      sessionStorage.removeItem(SESSION_KEY);
      return null;
    }
  }
}