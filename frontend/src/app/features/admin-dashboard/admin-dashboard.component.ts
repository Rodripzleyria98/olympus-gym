import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { API_BASE_URL } from '../../core/api.config';
import { AsignarRutinaModalComponent, SocioRutina } from './asignar-rutina-modal.component';

interface Socio {
  id: string;
  nombre: string;
  apellido: string;
  email: string;
  fechaRegistro: string;
  isActivo: boolean;
  estaAlDia: boolean;
  fechaVencimiento: string | null;
  diasRestantes: number;
}

interface Historial {
  membresias: { id: string; plan: string; fechaInicio: string; fechaFin: string; estado: string; pagos: { id: string; monto: number; estadoPago: string; fechaPago: string }[] }[];
}

@Component({
  selector: 'app-admin-dashboard',
  imports: [FormsModule, DatePipe, CurrencyPipe, AsignarRutinaModalComponent],
  template: `
    <section class="content-page">
      <div class="page-heading"><span class="eyebrow">GESTIÓN · SOCIOS</span><h1>Panel de socios</h1><p class="lede">Estado actual de membresías y pagos.</p></div>
      <div class="toolbar">
        <label class="search-field"><span aria-hidden="true">⌕</span><input aria-label="Buscar socio" placeholder="Buscar por nombre o correo" [(ngModel)]="search" (ngModelChange)="load()" /></label>
        <select aria-label="Filtrar por estado" [(ngModel)]="status" (ngModelChange)="load()"><option value="">Todos los estados</option><option value="activo">Activos</option><option value="inactivo">Inactivos</option></select>
        <span class="result-count">{{ socios().length }} socios</span>
      </div>
      @if (routineNotice()) { <div class="routine-assigned-toast" role="status">✓ {{ routineNotice() }}</div> }
      <div class="table-wrap"><table class="data-table"><thead><tr><th>Socio</th><th>Estado</th><th>Vencimiento</th><th>Días restantes</th><th>Acciones</th></tr></thead><tbody>
        @for (socio of socios(); track socio.id) {
          <tr><td><strong>{{ socio.nombre }} {{ socio.apellido }}</strong><small>{{ socio.email }}</small></td>
            <td><span class="status-badge" [class.status-active]="socio.isActivo" [class.status-inactive]="!socio.isActivo">{{ socio.isActivo ? 'ACTIVO' : 'INACTIVO' }}</span></td>
            <td>{{ socio.fechaVencimiento ? (socio.fechaVencimiento | date:'d MMM y') : 'Sin membresía' }}</td>
            <td>{{ socio.isActivo ? socio.diasRestantes : '—' }}</td>
            <td class="admin-member-actions"><button class="text-button" type="button" (click)="view(socio)">Ver historial</button><button class="routine-action-button" type="button" (click)="openRoutine(socio)" [attr.aria-label]="'Asignar / Gestionar Rutina para ' + socio.nombre + ' ' + socio.apellido"><span class="dumbbell-icon" aria-hidden="true"><i></i></span>Asignar / Gestionar Rutina</button></td>
          </tr>
        } @empty { <tr><td class="table-empty" colspan="5">No hay socios que coincidan con la búsqueda.</td></tr> }
      </tbody></table></div>
      @if (selected(); as member) {
        <section class="history-panel">
          <div class="heading-split"><div><span class="eyebrow">DETALLE DEL SOCIO</span><h2>{{ member.nombre }} {{ member.apellido }}</h2></div><button class="icon-button" type="button" aria-label="Cerrar historial" (click)="selected.set(null)">×</button></div>
          @if (history()?.membresias?.length) {
            @for (membership of history()?.membresias; track membership.id) {
              <div class="history-row"><div><strong>{{ membership.plan }}</strong><small>{{ membership.fechaInicio | date:'d MMM y' }} – {{ membership.fechaFin | date:'d MMM y' }}</small></div><span class="status-badge" [class.status-active]="membership.estado === 'Vigente'" [class.status-inactive]="membership.estado !== 'Vigente'">{{ membership.estado }}</span>
                @for (payment of membership.pagos; track payment.id) {
                  <div class="payment-row"><span>Pago {{ payment.fechaPago | date:'d MMM y' }}</span><strong>{{ payment.monto | currency:'MXN':'symbol':'1.0-0' }}</strong><span>{{ payment.estadoPago }}</span>
                    @if (payment.estadoPago === 'Pendiente') { <button class="text-button" type="button" (click)="approve(payment.id)">Validar pago</button> }
                  </div>
                }
              </div>
            }
          } @else { <p class="muted">Este socio todavía no tiene historial de membresías.</p> }
        </section>
      }
      @if (routineSocio(); as member) {
        <app-asignar-rutina-modal [socio]="member" (close)="closeRoutine()" (assigned)="onRoutineAssigned()" />
      }
    </section>
  `,
})
export class AdminDashboardComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly socios = signal<Socio[]>([]);
  readonly selected = signal<Socio | null>(null);
  readonly history = signal<Historial | null>(null);
  readonly routineSocio = signal<SocioRutina | null>(null);
  readonly routineNotice = signal('');
  search = '';
  status = '';
  private searchTimer?: ReturnType<typeof setTimeout>;
  private noticeTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void { this.load(); }

  load(): void {
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      const params = new URLSearchParams();
      if (this.search.trim()) params.set('buscar', this.search.trim());
      if (this.status) params.set('estado', this.status);
      this.http.get<Socio[]>(`${API_BASE_URL}/admin/usuarios?${params}`).subscribe({ next: (rows) => this.socios.set(rows) });
    }, 180);
  }

  view(socio: Socio): void {
    this.selected.set(socio);
    this.history.set(null);
    this.http.get<Historial>(`${API_BASE_URL}/admin/usuarios/${socio.id}/historial`).subscribe({ next: (history) => this.history.set(history) });
  }

  openRoutine(socio: Socio): void {
    this.routineSocio.set({ id: socio.id, nombre: socio.nombre, apellido: socio.apellido, email: socio.email });
  }

  closeRoutine(): void {
    this.routineSocio.set(null);
  }

  onRoutineAssigned(): void {
    const socio = this.routineSocio();
    if (!socio) return;
    this.routineSocio.set(null);
    this.routineNotice.set(`Rutina asignada correctamente a ${socio.nombre} ${socio.apellido}`);
    this.load();
    clearTimeout(this.noticeTimer);
    this.noticeTimer = setTimeout(() => this.routineNotice.set(''), 5000);
  }

  approve(paymentId: string): void {
    const selected = this.selected();
    if (!selected) return;
    this.http.put(`${API_BASE_URL}/admin/usuarios/${selected.id}/estado`, { pagoId: paymentId, estadoPago: 'Aprobado' })
      .subscribe({ next: () => { this.view(selected); this.load(); } });
  }
}