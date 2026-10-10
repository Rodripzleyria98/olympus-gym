import { HttpClient, HttpParams } from '@angular/common/http';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { normalizePagedResponse, PagedResponse } from '../../core/paged-response';
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
    <section class="content-page" [class.routine-modal-open]="routineSocio() !== null">
      <div class="page-heading"><span class="eyebrow">GESTIÓN · SOCIOS</span><h1>Panel de socios</h1><p class="lede">Estado actual de membresías y pagos.</p></div>
      <div class="toolbar">
        <label class="search-field"><span aria-hidden="true">⌕</span><input aria-label="Buscar socio" placeholder="Buscar por nombre o correo" [(ngModel)]="search" (ngModelChange)="onFiltersChange()" /></label>
        <select aria-label="Filtrar por estado" [(ngModel)]="status" (ngModelChange)="onFiltersChange()"><option value="">Todos los estados</option><option value="activo">Activos</option><option value="inactivo">Inactivos</option></select>
        <span class="result-count">{{ socios().length }} de {{ totalCount() }} socios · página {{ page() }} de {{ totalPages() || 1 }}</span>
      </div>
      @if (loadError()) { <p class="form-error" role="alert">{{ loadError() }}</p> }
      @if (routineNotice()) { <div class="routine-assigned-toast" role="status">✓ {{ routineNotice() }}</div> }
      <div class="table-wrap"><table class="data-table admin-socios-table"><thead><tr><th>Socio</th><th>Estado</th><th>Vencimiento</th><th>Días restantes</th></tr></thead><tbody>
        @for (socio of socios(); track socio.id) {
          <tr [class.is-expanded]="expandedSocioId() === socio.id"><td>
            <button class="member-select" type="button" [attr.aria-expanded]="expandedSocioId() === socio.id" (click)="toggleSocioActions(socio.id)">
              <span><strong>{{ socio.nombre }} {{ socio.apellido }}</strong><small>{{ socio.email }}</small></span>
              <span class="member-expand-indicator" aria-hidden="true">{{ expandedSocioId() === socio.id ? '−' : '+' }}</span>
            </button>
            @if (expandedSocioId() === socio.id) {
              <div class="member-expanded-actions">
                <div class="member-mobile-details">
                  <div><span>Estado</span><strong>{{ socio.isActivo ? 'Activo' : 'Inactivo' }}</strong></div>
                  <div><span>Vencimiento</span><strong>{{ socio.fechaVencimiento ? (socio.fechaVencimiento | date:'d MMM y') : 'Sin membresía' }}</strong></div>
                  <div><span>Días restantes</span><strong>{{ socio.isActivo ? socio.diasRestantes : '—' }}</strong></div>
                </div>
                <div class="admin-member-actions"><button class="text-button" type="button" (click)="view(socio)">Ver historial</button><button class="routine-action-button" type="button" (click)="openRoutine(socio)"><span class="dumbbell-icon" aria-hidden="true"><i></i></span>Asignar / Gestionar Rutina</button></div>
              </div>
            }
          </td>
            <td><span class="status-badge" [class.status-active]="socio.isActivo" [class.status-inactive]="!socio.isActivo">{{ socio.isActivo ? 'ACTIVO' : 'INACTIVO' }}</span></td>
            <td>{{ socio.fechaVencimiento ? (socio.fechaVencimiento | date:'d MMM y') : 'Sin membresía' }}</td>
            <td>{{ socio.isActivo ? socio.diasRestantes : '—' }}</td>
          </tr>
        } @empty { <tr><td class="table-empty" colspan="4">{{ loadError() ? 'No se pudo cargar la lista de socios.' : 'No hay socios que coincidan con la búsqueda.' }}</td></tr> }
      </tbody></table></div>
      @if (totalPages() > 1) {
        <nav class="pagination" aria-label="Paginación de socios">
          <button type="button" (click)="goToPage(page() - 1)" [disabled]="page() <= 1">Anterior</button>
          @for (pageNumber of pageNumbers(); track pageNumber) {
            <button type="button" [class.current]="pageNumber === page()" [attr.aria-current]="pageNumber === page() ? 'page' : null" (click)="goToPage(pageNumber)">{{ pageNumber }}</button>
          }
          <button type="button" (click)="goToPage(page() + 1)" [disabled]="page() >= totalPages()">Siguiente</button>
        </nav>
      }
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
  private readonly destroyRef = inject(DestroyRef);
  private readonly filterChanges = new Subject<string>();
  readonly socios = signal<Socio[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly totalCount = signal(0);
  readonly pageSize = 25;
  readonly loadError = signal('');
  readonly selected = signal<Socio | null>(null);
  readonly history = signal<Historial | null>(null);
  readonly routineSocio = signal<SocioRutina | null>(null);
  readonly expandedSocioId = signal<string | null>(null);
  readonly routineNotice = signal('');
  search = '';
  status = '';
  private noticeTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.filterChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => this.load());
    this.load();
  }

  load(): void {
    let params = new HttpParams().set('page', this.page()).set('pageSize', this.pageSize);
    const search = this.search.trim();
    if (search) params = params.set('buscar', search);
    if (this.status) params = params.set('estado', this.status);
    this.loadError.set('');
    this.http.get<PagedResponse<Socio> | Socio[]>(`${API_BASE_URL}/admin/usuarios`, { params }).subscribe({
      next: (response) => {
        const result = normalizePagedResponse(response, this.page(), this.pageSize);
        this.socios.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: () => {
        this.socios.set([]);
        this.totalCount.set(0);
        this.totalPages.set(0);
        this.loadError.set('No se pudo cargar la lista de socios. Intenta de nuevo.');
      },
    });
  }

  onFiltersChange(): void {
    this.page.set(1);
    this.filterChanges.next(`${this.search.trim()}|${this.status}`);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) return;
    this.page.set(page);
    this.load();
  }

  pageNumbers(): number[] {
    const first = Math.max(1, this.page() - 2);
    const last = Math.min(this.totalPages(), this.page() + 2);
    return Array.from({ length: last - first + 1 }, (_, index) => first + index);
  }

  toggleSocioActions(socioId: string): void {
    this.expandedSocioId.update(current => current === socioId ? null : socioId);
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