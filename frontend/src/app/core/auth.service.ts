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
  rol: 'Admin' | 'User';
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
  readonly isAdmin = computed(() => this.currentUser()?.rol === 'Admin');

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/login`, { email, password }).pipe(
      tap((response) => this.saveSession(response)),
    );
  }

  register(payload: { nombre: string; apellido: string; email: string; password: string }) {
    return this.http.post<UsuarioSesion>(`${API_BASE_URL}/auth/register`, payload);
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