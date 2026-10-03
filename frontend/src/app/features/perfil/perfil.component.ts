import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { API_BASE_URL } from '../../core/api.config';

interface Perfil {
  usuario: { nombre: string; apellido: string; email: string; fechaRegistro?: string };
  isActivo: boolean;
  estadoMembresia: string;
  plan: string | null;
  fechaInicio: string | null;
  fechaVencimiento: string | null;
  diasRestantes: number;
  estadoPago: string | null;
  montoPago: number | null;
}

@Component({
  selector: 'app-perfil',
  imports: [DatePipe, RouterLink],
  template: `
    <section class="content-page profile-page">
      @if (profile(); as data) {
        <div class="page-heading"><span class="eyebrow">CUENTA DEL SOCIO</span><h1>Hola, {{ data.usuario.nombre }}.</h1><p class="lede">Tu actividad Olympus, de un vistazo.</p></div>
        <div class="profile-grid">
          <section class="profile-details"><span class="eyebrow">DATOS PERSONALES</span><h2>{{ data.usuario.nombre }} {{ data.usuario.apellido }}</h2><p>{{ data.usuario.email }}</p>
            @if (data.fechaInicio) { <div class="detail-line"><span>Socio desde</span><strong>{{ data.fechaInicio | date:'d MMMM y' }}</strong></div> }
          </section>
          <section class="membership-card" [class.membership-inactive]="!data.isActivo">
            <div class="membership-card-top"><span class="eyebrow">MEMBRESÍA ACTUAL</span><span class="status-badge" [class.status-active]="data.isActivo" [class.status-inactive]="!data.isActivo">{{ data.isActivo ? 'ACTIVA' : 'INACTIVA' }}</span></div>
            <h2>{{ data.plan ?? 'Sin membresía' }}</h2>
            @if (data.fechaVencimiento) {
              <div class="expiry-block"><span>FECHA DE CORTE</span><strong>{{ data.fechaVencimiento | date:'d MMMM y' }}</strong><small>{{ data.isActivo ? data.diasRestantes + ' días restantes' : 'Membresía vencida o pendiente' }}</small></div>
            } @else { <p class="muted">Elige un plan para activar tu membresía.</p> }
            <div class="membership-footer"><span>Pago: {{ data.estadoPago ?? 'Sin pagos' }}</span><a class="button button-light" routerLink="/membresias">{{ data.isActivo ? 'Renovar plan' : 'Ver planes' }}</a></div>
          </section>
        </div>
      } @else if (errorMessage()) {
        <div class="empty-state"><h2>No pudimos cargar tu perfil.</h2><p>{{ errorMessage() }}</p><button class="button button-dark" type="button" (click)="load()">Intentar de nuevo</button></div>
      } @else { <div class="loading-state">Cargando perfil…</div> }
    </section>
  `,
})
export class PerfilComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly profile = signal<Perfil | null>(null);
  readonly errorMessage = signal('');

  ngOnInit(): void { this.load(); }

  load(): void {
    this.errorMessage.set('');
    this.http.get<Perfil>(`${API_BASE_URL}/perfil/mi-estado`).subscribe({
      next: (profile) => this.profile.set(profile),
      error: () => this.errorMessage.set('Intenta cargar el perfil de nuevo.'),
    });
  }
}