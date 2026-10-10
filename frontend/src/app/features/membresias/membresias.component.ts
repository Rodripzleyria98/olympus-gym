import { DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { API_BASE_URL } from '../../core/api.config';
import { EstadoPago, PagosService } from './pagos.service';

interface PlanMembresia {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  duracionDias: number;
  clasesIncluidas: number | null;
}

interface PagoConfirmado {
  pagoId: string;
  membresiaUsuarioId: string;
  planNombre: string;
  monto: number;
  fechaInicio: string;
  fechaFin: string;
  estadoPago: string;
}

interface EstadoSocio {
  isActivo: boolean;
}

@Component({
  selector: 'app-membresias',
  imports: [FormsModule, DatePipe, RouterLink],
  template: `
    <section class="content-page memberships-page">
      <header class="memberships-heading">
        <span class="eyebrow">OLYMPUS GYM · SUSCRIPCIONES</span>
        <h1>Membresías y pagos</h1>
        <p class="lede">Elige el pase que mejor se adapta a tu ritmo.</p>
      </header>

      @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
      @if (paymentReturnError() && !isPaymentReturn()) { <p class="form-error" role="alert">{{ paymentReturnError() }}</p> }
      @if (paymentReturn(); as payment) {
        <section class="payment-confirmation" aria-live="polite">
          <span class="eyebrow">{{ payment.estadoPago === 'Aprobado' ? 'PAGO APROBADO' : payment.estadoPago === 'Pendiente' ? 'PAGO EN PROCESO' : 'PAGO NO COMPLETADO' }}</span>
          <h2>{{ payment.planNombre }}</h2>
          <p>{{ payment.estadoPago === 'Aprobado' ? 'El pago fue confirmado y tu membresía está activa.' : payment.estadoPago === 'Pendiente' ? 'Esperando confirmación de Mercado Pago. La membresía se activará al recibirla.' : 'Mercado Pago no confirmó este pago.' }}</p>
          <div class="confirmation-meta"><span>Importe</span><strong>$ {{ formatPrice(payment.monto) }}</strong></div>
          @if (payment.estadoPago === 'Pendiente') {
            <button class="button button-outline" type="button" [disabled]="statusLoading()" (click)="refreshPaymentStatus()">
              {{ statusLoading() ? 'Consultando…' : 'Actualizar estado' }}
            </button>
          }
          @if (paymentReturnError()) { <p class="form-error" role="alert">{{ paymentReturnError() }}</p> }
          <a class="button button-outline" routerLink="/membresias">Volver a membresías</a>
        </section>
      } @else if (isPaymentReturn()) {
        <section class="payment-confirmation" aria-live="polite">
          <span class="eyebrow">ESTADO DEL PAGO</span>
          <h2>{{ statusLoading() ? 'Consultando pago…' : 'No pudimos confirmar el pago' }}</h2>
          <p>{{ paymentReturnError() || 'El estado se actualizará cuando Mercado Pago confirme la operación.' }}</p>
          <a class="button button-outline" routerLink="/membresias">Volver a membresías</a>
        </section>
      } @else if (success(); as payment) {
        <section class="payment-confirmation" aria-live="polite">
          <span class="payment-confirmation-mark" aria-hidden="true">✓</span>
          <span class="eyebrow">PAGO APROBADO</span>
          <h2>{{ payment.planNombre }}</h2>
          <p>Tu acceso está activo desde {{ payment.fechaInicio | date:'d MMM y':'UTC' }} hasta {{ payment.fechaFin | date:'d MMM y':'UTC' }}.</p>
          <div class="confirmation-meta"><span>Importe</span><strong>$ {{ formatPrice(payment.monto) }}</strong></div>
          <button class="button button-outline" type="button" (click)="success.set(null)">Volver a las membresías</button>
        </section>
      } @else {
        <div class="membership-grid">
          @for (plan of plans(); track plan.id; let index = $index) {
            <article class="membership-card-new" [class.membership-card-featured]="index === 0">
              <div class="membership-card-topline">
                <span class="membership-index">PASE {{ (index + 1).toString().padStart(2, '0') }}</span>
                <span class="classes-badge">{{ plan.clasesIncluidas === null ? 'ILIMITADAS' : plan.clasesIncluidas + ' CLASES' }}</span>
              </div>
              <h2>{{ plan.nombre }}</h2>
              <div class="membership-price"><span>$</span><strong>{{ formatPrice(plan.precio) }}</strong><small>/ mes</small></div>
              <p class="membership-description">{{ plan.descripcion }}</p>
              <div class="membership-card-footer">
                <span class="membership-duration">{{ plan.duracionDias }} días de acceso</span>
                <button class="button button-neon" type="button" (click)="openPayment(plan)">
                  {{ hasActiveMembership() ? 'Renovar Pase' : 'Adquirir Membresía' }}
                </button>
                @if (auth.isAdmin()) {
                  <button class="edit-rate-button" type="button" (click)="openEdit(plan)">Editar Tarifa</button>
                }
              </div>
            </article>
          } @empty {
            @if (!loading()) { <p class="muted">No hay membresías activas disponibles.</p> }
            @else { <p class="muted">Cargando pases Olympus…</p> }
          }
        </div>
      }

      @if (paymentPlan(); as plan) {
        <div class="modal-backdrop" (click)="closePayment()" (keydown.escape)="closePayment()">
          <section class="membership-modal" role="dialog" aria-modal="true" aria-labelledby="payment-modal-title" (click)="$event.stopPropagation()">
            <button class="modal-close" type="button" aria-label="Cerrar" (click)="closePayment()">×</button>
            <span class="eyebrow">CHECKOUT OLYMPUS</span>
            <h2 id="payment-modal-title">Confirma tu pase</h2>
            <div class="payment-summary">
              <span>{{ plan.nombre }}</span>
              <strong>$ {{ formatPrice(plan.precio) }} <small>/ mes</small></strong>
              <span>{{ plan.clasesIncluidas === null ? 'Clases ilimitadas' : plan.clasesIncluidas + ' clases incluidas' }}</span>
            </div>
            @if (auth.isAdmin()) {
              <label>UUID del socio<input name="usuarioId" [(ngModel)]="adminUserId" placeholder="00000000-0000-0000-0000-000000000000" required /></label>
            }
            <label>Método de pago
              <select name="metodoPago" [(ngModel)]="metodoPago">
                <option value="Transferencia">Transferencia bancaria</option>
                <option value="Efectivo">Efectivo en recepción</option>
              </select>
            </label>
            @if (metodoPago === 'Transferencia') {
              <p class="transfer-note">Solicita los datos bancarios oficiales en recepción. Esta confirmación corresponde al entorno de prueba de Olympus.</p>
            }
            <p class="simulation-note">La transferencia o el efectivo se registrarán como pago de prueba. Para pagar en línea, usa Mercado Pago.</p>
            @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
            <button class="button button-neon button-full" type="button" [disabled]="saving()" (click)="confirmPayment(plan)">
              {{ saving() ? 'Procesando…' : 'Registrar pago de prueba' }}
            </button>
            @if (!auth.isAdmin()) {
              <button class="button button-outline button-full" type="button" [disabled]="saving()" (click)="payWithMercadoPago(plan)">
                Pagar con Mercado Pago
              </button>
            }
          </section>
        </div>
      }

      @if (editingPlan(); as plan) {
        <div class="modal-backdrop" (click)="closeEdit()" (keydown.escape)="closeEdit()">
          <form class="membership-modal" (ngSubmit)="saveRate(plan)" role="dialog" aria-modal="true" aria-labelledby="rate-modal-title" (click)="$event.stopPropagation()">
            <button class="modal-close" type="button" aria-label="Cerrar" (click)="closeEdit()">×</button>
            <span class="eyebrow">GESTIÓN DE TARIFAS</span>
            <h2 id="rate-modal-title">Editar membresía</h2>
            <label>Nombre<input name="nombre" [(ngModel)]="editForm.nombre" maxlength="100" required /></label>
            <label>Descripción<textarea name="descripcion" [(ngModel)]="editForm.descripcion" rows="4" maxlength="500" required></textarea></label>
            <label>Precio mensual<input name="precio" type="number" min="1" step="1" [(ngModel)]="editForm.precio" required /></label>
            @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
            <button class="button button-neon button-full" type="submit" [disabled]="saving()">{{ saving() ? 'Guardando…' : 'Guardar tarifa' }}</button>
          </form>
        </div>
      }
    </section>
  `,
})
export class MembresiasComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly pagosService = inject(PagosService);
  readonly auth = inject(AuthService);
  readonly plans = signal<PlanMembresia[]>([]);
  readonly paymentPlan = signal<PlanMembresia | null>(null);
  readonly editingPlan = signal<PlanMembresia | null>(null);
  readonly success = signal<PagoConfirmado | null>(null);
  readonly paymentReturn = signal<EstadoPago | null>(null);
  readonly paymentReturnError = signal('');
  readonly isPaymentReturn = signal(false);
  readonly statusLoading = signal(false);
  readonly errorMessage = signal('');
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly activeMembership = signal(false);
  metodoPago = 'Transferencia';
  adminUserId = '';
  editForm = { nombre: '', descripcion: '', precio: 0 };

  ngOnInit(): void {
    this.loadPlans();
    this.isPaymentReturn.set(this.route.snapshot.routeConfig?.path?.startsWith('pagos/') ?? false);
    this.route.queryParamMap.subscribe(params => {
      const externalReference = params.get('external_reference');
      if (externalReference) this.checkPaymentStatus(externalReference);
      else if (this.isPaymentReturn()) this.paymentReturnError.set('No se recibió la referencia del pago.');
    });
    this.http.get<EstadoSocio>(`${API_BASE_URL}/perfil/mi-estado`).subscribe({
      next: profile => this.activeMembership.set(profile.isActivo),
    });
  }

  formatPrice(amount: number): string {
    return new Intl.NumberFormat('es-AR', { maximumFractionDigits: 0 }).format(amount);
  }

  hasActiveMembership(): boolean {
    return this.activeMembership();
  }

  openPayment(plan: PlanMembresia): void {
    this.errorMessage.set('');
    this.metodoPago = 'Transferencia';
    this.adminUserId = '';
    this.paymentPlan.set(plan);
  }

  closePayment(): void {
    this.paymentPlan.set(null);
    this.errorMessage.set('');
  }

  confirmPayment(plan: PlanMembresia): void {
    if (this.saving()) return;
    if (this.auth.isAdmin() && !this.adminUserId.trim()) {
      this.errorMessage.set('Indica el UUID del socio para registrar el pago.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    const body: { planMembresiaId: number; metodoPago: string; usuarioId?: string } = {
      planMembresiaId: plan.id,
      metodoPago: this.metodoPago,
    };
    if (this.auth.isAdmin()) body.usuarioId = this.adminUserId.trim();

    this.http.post<PagoConfirmado>(`${API_BASE_URL}/pagos/checkout`, body).subscribe({
      next: (payment) => {
        this.paymentPlan.set(null);
        this.success.set(payment);
        if (!this.auth.isAdmin()) this.activeMembership.set(true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'No se pudo registrar el pago.');
        this.saving.set(false);
      },
      complete: () => this.saving.set(false),
    });
  }

  payWithMercadoPago(plan: PlanMembresia): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set('');
    this.pagosService.iniciarRenovacion(plan.id).subscribe({
      next: (preference) => {
        const isLocal = ['localhost', '127.0.0.1'].includes(window.location.hostname);
        const checkoutUrl = isLocal
          ? preference.sandboxInitPoint || preference.initPoint
          : preference.initPoint;
        if (!checkoutUrl) {
          this.errorMessage.set('Mercado Pago no devolvió una URL de pago.');
          this.saving.set(false);
          return;
        }
        window.location.assign(checkoutUrl);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'No se pudo iniciar el pago con Mercado Pago.');
        this.saving.set(false);
      },
    });
  }

  refreshPaymentStatus(): void {
    const paymentId = this.route.snapshot.queryParamMap.get('external_reference');
    if (paymentId) this.checkPaymentStatus(paymentId);
  }

  private checkPaymentStatus(paymentId: string): void {
    if (!/^[0-9a-f-]{36}$/i.test(paymentId)) {
      this.paymentReturnError.set('No se pudo identificar el pago que regresó de Mercado Pago.');
      return;
    }

    this.statusLoading.set(true);
    this.paymentReturnError.set('');
    this.pagosService.obtenerEstado(paymentId).subscribe({
      next: payment => {
        this.paymentReturn.set(payment);
        if (payment.estadoPago === 'Aprobado') this.activeMembership.set(true);
        this.statusLoading.set(false);
      },
      error: () => {
        this.paymentReturnError.set('No se pudo consultar el pago. Inicia sesión con la cuenta que realizó la compra.');
        this.statusLoading.set(false);
      },
    });
  }

  openEdit(plan: PlanMembresia): void {
    this.errorMessage.set('');
    this.editForm = { nombre: plan.nombre, descripcion: plan.descripcion, precio: plan.precio };
    this.editingPlan.set(plan);
  }

  closeEdit(): void {
    this.editingPlan.set(null);
    this.errorMessage.set('');
  }

  saveRate(plan: PlanMembresia): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.http.put<PlanMembresia>(`${API_BASE_URL}/admin/membresias/${plan.id}`, this.editForm).subscribe({
      next: (updated) => {
        this.plans.update(plans => plans.map(item => item.id === updated.id ? updated : item));
        this.closeEdit();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'No se pudo actualizar la tarifa.');
        this.saving.set(false);
      },
      complete: () => this.saving.set(false),
    });
  }

  private loadPlans(): void {
    this.loading.set(true);
    this.http.get<PlanMembresia[]>(`${API_BASE_URL}/membresias`).subscribe({
      next: (plans) => this.plans.set(plans),
      error: () => this.errorMessage.set('No se pudieron cargar las membresías.'),
      complete: () => this.loading.set(false),
    });
  }
}