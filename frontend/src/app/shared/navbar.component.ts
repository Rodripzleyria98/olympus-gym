import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <header class="topbar">
      <a class="brand" routerLink="/foro" aria-label="Olympus Gym, inicio">
        <span class="brand-mark">O</span>
        <span class="brand-lockup"><strong>OLYMPUS</strong><small><i></i>G Y M<i></i></small></span>
      </a>
      @if (auth.currentUser(); as user) {
        <nav class="main-nav" aria-label="Navegación principal">
          <a routerLink="/foro" routerLinkActive="active">Inicio</a>
          @if (auth.isAdmin()) {
            <a routerLink="/admin" routerLinkActive="active">Socios</a>
            <a routerLink="/membresias" routerLinkActive="active">Membresías y Pagos</a>
          } @else {
            <a routerLink="/rutina" routerLinkActive="active">Mi Rutina</a>
            <a routerLink="/membresias" routerLinkActive="active">Membresía y Pagos</a>
            <a routerLink="/perfil" routerLinkActive="active">Mi Perfil</a>
          }
        </nav>
        <div class="account-menu">
          <span class="account-name">{{ user.nombre }}</span>
          <button class="button button-quiet button-small" type="button" (click)="auth.logout()">Salir</button>
        </div>
      } @else {
        <a class="button button-dark button-small" routerLink="/login">Iniciar sesión</a>
      }
    </header>
  `,
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
}