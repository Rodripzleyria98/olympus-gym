import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { AuthService } from '../../core/auth.service';

type SeccionOperacion = 'caja' | 'socios' | 'cobros' | 'pases' | 'ventas' | 'stock' | 'historial';
type TipoOtorgamiento = 'Efectivo' | 'Bonificada';

interface CajaTurno {
  id: number;
  fecha: string;
  turno: string;
  responsableTurno: string;
  montoInicial: number;
  montoFinalEfectivoReal: number | null;
  observaciones: string | null;
  estado: string;
}

interface CajaVenta {
  id: number;
  fecha: string;
  montoTotal: number;
  metodoPago: string;
  items: { producto: string; cantidad: number; precioUnitario: number }[];
}

interface CajaPase {
  id: number;
  nombreCompleto: string;
  dni: string;
  fecha: string;
  monto: number;
  metodoPago: string;
}

interface CajaCobroMembresia {
  id: string;
  socio: string;
  plan: string;
  fecha: string;
  monto: number;
  metodoPago: string;
  estadoPago: string;
}

interface CajaDetalle {
  caja: CajaTurno;
  cobrosMembresia: CajaCobroMembresia[];
  ventas: CajaVenta[];
  pases: CajaPase[];
}

interface CajaActual {
  fecha: string;
  turno: string;
  detalle: CajaDetalle | null;
}

interface Producto {
  id: number;
  nombre: string;
  stockActual: number;
  stockMinimo: number;
  precio: number;
  codigo: string | null;
}

interface Socio {
  id: string;
  nombre: string;
  apellido: string;
  email: string;
  dni: string;
}

interface PlanMembresia {
  id: number;
  nombre: string;
  precio: number;
  duracionDias: number;
}

@Component({
  selector: 'app-operaciones',
  imports: [FormsModule, CurrencyPipe, DatePipe],
  template: `
    <section class="content-page">
      <div class="page-heading">
        <span class="eyebrow">OPERACIÓN DEL GIMNASIO</span>
        <h1>Operaciones</h1>
        <p class="lede">Elige una sección para gestionar cada tarea.</p>
      </div>

      <nav class="toolbar" aria-label="Secciones de operaciones">
        @if (auth.isAdmin()) {
          <button class="button button-small" [class.button-accent]="activeSection() === 'caja'" type="button" (click)="setSection('caja')">Caja</button>
          <button class="button button-small" [class.button-accent]="activeSection() === 'socios'" type="button" (click)="setSection('socios')">Socios y membresías</button>
          <button class="button button-small" [class.button-accent]="activeSection() === 'historial'" type="button" (click)="setSection('historial')">Historial de caja</button>
          <button class="button button-small" [class.button-accent]="activeSection() === 'stock'" type="button" (click)="setSection('stock')">Inventario</button>
        }
        <button class="button button-small" [class.button-accent]="activeSection() === 'cobros'" type="button" (click)="setSection('cobros')">Cobrar membresía</button>
        <button class="button button-small" [class.button-accent]="activeSection() === 'pases'" type="button" (click)="setSection('pases')">Pases diarios</button>
        <button class="button button-small" [class.button-accent]="activeSection() === 'ventas'" type="button" (click)="setSection('ventas')">Ventas</button>
      </nav>

      @if (notice()) { <div class="routine-assigned-toast" role="status">{{ notice() }}</div> }
      @if (error()) { <p class="form-error" role="alert">{{ error() }}</p> }

      @if (activeSection() === 'caja' && auth.isAdmin()) {
        <section class="history-panel">
          <div class="heading-split">
            <div>
              <span class="eyebrow">CAJA DEL TURNO</span>
              <h2>{{ cajaActual()?.turno ?? 'Turno actual' }} · {{ cajaActual()?.fecha | date:'d MMM y' }}</h2>
            </div>
            <span class="status-badge" [class.status-active]="cajaActual()?.detalle" [class.status-inactive]="!cajaActual()?.detalle">
              {{ cajaActual()?.detalle ? 'ABIERTA' : 'CERRADA' }}
            </span>
          </div>
          @if (cajaActual()?.detalle; as detail) {
            <div class="profile-grid">
              <div class="detail-line"><span>Responsable</span><strong>{{ detail.caja.responsableTurno }}</strong></div>
              <div class="detail-line"><span>Monto de apertura</span><strong>{{ detail.caja.montoInicial | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
            </div>
            <h3>Cobros de membresía del turno</h3>
            <div class="table-wrap"><table class="data-table"><thead><tr><th>Socio</th><th>Plan</th><th>Fecha</th><th>Monto</th><th>Método</th><th>Estado</th></tr></thead><tbody>
              @for (payment of detail.cobrosMembresia; track payment.id) {
                <tr><td>{{ payment.socio }}</td><td>{{ payment.plan }}</td><td>{{ payment.fecha | date:'short' }}</td><td>{{ payment.monto | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ payment.metodoPago }}</td><td>{{ payment.estadoPago }}</td></tr>
              } @empty { <tr><td colspan="6" class="table-empty">No hay cobros de membresía en este turno.</td></tr> }
            </tbody></table></div>
            <h3>Ventas del turno</h3>
            <div class="table-wrap"><table class="data-table"><thead><tr><th>Fecha</th><th>Productos</th><th>Pago</th><th>Total</th></tr></thead><tbody>
              @for (sale of detail.ventas; track sale.id) {
                <tr><td>{{ sale.fecha | date:'short' }}</td><td>@for (item of sale.items; track $index) { <span>{{ item.cantidad }} × {{ item.producto }}@if (!$last) {, }</span> }</td><td>{{ sale.metodoPago }}</td><td>{{ sale.montoTotal | currency:'ARS':'symbol':'1.0-2' }}</td></tr>
              } @empty { <tr><td colspan="4" class="table-empty">No hay ventas de productos en este turno.</td></tr> }
            </tbody></table></div>
            <h3>Pases diarios registrados en el turno</h3>
            <div class="table-wrap"><table class="data-table"><thead><tr><th>Persona</th><th>DNI</th><th>Fecha</th><th>Monto</th><th>Pago</th></tr></thead><tbody>
              @for (pass of detail.pases; track pass.id) {
                <tr><td>{{ pass.nombreCompleto }}</td><td>{{ pass.dni }}</td><td>{{ pass.fecha | date:'short' }}</td><td>{{ pass.monto | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ pass.metodoPago }}</td></tr>
              } @empty { <tr><td colspan="5" class="table-empty">No hay pases diarios en este turno.</td></tr> }
            </tbody></table></div>
            <form class="editor-panel" (ngSubmit)="closeCash()">
              <h3>Cerrar caja</h3>
              <label>Observaciones (opcional)<textarea name="cashNotes" [(ngModel)]="cashNotes" rows="2" maxlength="2000"></textarea></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Cerrar turno</button>
            </form>
          } @else {
            <form class="editor-panel" (ngSubmit)="openCash()">
              <h3>Abrir caja · {{ cajaActual()?.turno }}</h3>
              <label>Persona responsable<input name="cashResponsible" [(ngModel)]="responsableTurno" maxlength="150" minlength="2" autocomplete="name" required /></label>
              <label>Monto inicial de caja<input type="number" name="cashOpening" min="0" step="0.01" [(ngModel)]="openingAmount" required /></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Abrir caja</button>
            </form>
          }
        </section>
      }

      @if (activeSection() === 'socios' && auth.isAdmin()) {
        <div class="profile-grid">
          <section class="history-panel">
            <span class="eyebrow">ALTA ADMINISTRATIVA</span><h2>Registrar socio con membresía</h2>
            <form class="editor-panel" (ngSubmit)="createMemberWithPlan()">
              <div class="form-row">
                <label>Nombre<input name="newMemberName" [(ngModel)]="newMember.nombre" required maxlength="100" /></label>
                <label>Apellido<input name="newMemberLastName" [(ngModel)]="newMember.apellido" required maxlength="100" /></label>
              </div>
              <label>DNI<input name="newMemberDni" [(ngModel)]="newMember.dni" pattern="\\d{7,12}" inputmode="numeric" maxlength="12" required /></label>
              <label>Correo<input name="newMemberEmail" [(ngModel)]="newMember.email" type="email" maxlength="254" required /></label>
              <label>Contraseña inicial<input name="newMemberPassword" [(ngModel)]="newMember.password" type="password" pattern="(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9\\s]).{8,100}" maxlength="100" required /><small>Usa al menos 8 caracteres, una mayúscula, un número y un carácter especial.</small></label>
              <label>Plan<select name="newMemberPlan" [(ngModel)]="newMember.planMembresiaId" required>
                <option [ngValue]="null">Selecciona el plan</option>
                @for (plan of plans(); track plan.id) { <option [ngValue]="plan.id">{{ plan.nombre }} · {{ plan.duracionDias }} días · {{ plan.precio | currency:'ARS':'symbol':'1.0-2' }}</option> }
              </select></label>
              <label>Otorgamiento
                <select name="newMemberGrantType" [(ngModel)]="newMember.tipoOtorgamiento">
                  <option value="Efectivo">Cobrar en efectivo</option><option value="Bonificada">Otorgar bonificada</option>
                </select>
              </label>
              <button class="button button-dark" type="submit" [disabled]="busy() || !newMember.planMembresiaId">Crear socio y asignar plan</button>
            </form>
          </section>

          <section class="history-panel">
            <span class="eyebrow">SOCIOS REGISTRADOS</span><h2>Asignar membresía</h2>
            <form class="editor-panel" (ngSubmit)="findMember()">
              <label>Buscar socio por DNI<input name="assignMemberDni" [(ngModel)]="memberSearchDni" pattern="\\d{7,12}" inputmode="numeric" maxlength="12" required /></label>
              <button class="button button-quiet" type="submit" [disabled]="busy()">Buscar socio</button>
            </form>
            @if (members().length) {
              <form class="editor-panel" (ngSubmit)="assignMembership()">
                <label>Socio<select name="selectedMember" [(ngModel)]="memberId" required>
                  <option value="">Selecciona un socio</option>
                  @for (member of members(); track member.id) { <option [value]="member.id">{{ member.nombre }} {{ member.apellido }} · {{ member.email }}</option> }
                </select></label>
                <label>Plan<select name="assignPlan" [(ngModel)]="planId" required>
                  <option [ngValue]="null">Selecciona el plan</option>
                  @for (plan of plans(); track plan.id) { <option [ngValue]="plan.id">{{ plan.nombre }} · {{ plan.duracionDias }} días · {{ plan.precio | currency:'ARS':'symbol':'1.0-2' }}</option> }
                </select></label>
                <label>Otorgamiento<select name="assignGrantType" [(ngModel)]="membershipGrantType">
                  <option value="Bonificada">Otorgar bonificada</option><option value="Efectivo">Cobrar en efectivo</option>
                </select></label>
                <button class="button button-dark" type="submit" [disabled]="busy() || !planId">Asignar membresía</button>
              </form>
            }
          </section>
        </div>
      }

      @if (activeSection() === 'cobros') {
        <section class="history-panel">
          <span class="eyebrow">COBRO EN EFECTIVO</span><h2>Renovar o cobrar membresía</h2>
          <p>El cobro en efectivo se registra dentro de la caja abierta del turno actual.</p>
          <form class="editor-panel" (ngSubmit)="findMember()">
            <label>DNI del socio<input name="paymentDni" [(ngModel)]="memberSearchDni" pattern="\\d{7,12}" inputmode="numeric" maxlength="12" required /></label>
            <button class="button button-quiet" type="submit" [disabled]="busy()">Buscar socio</button>
          </form>
          @if (members().length) {
            <form class="editor-panel" (ngSubmit)="registerCashPayment()">
              <label>Socio<select name="paymentMember" [(ngModel)]="memberId" required>
                <option value="">Selecciona un socio</option>
                @for (member of members(); track member.id) { <option [value]="member.id">{{ member.nombre }} {{ member.apellido }} · DNI {{ member.dni }}</option> }
              </select></label>
              <label>Plan<select name="paymentPlan" [(ngModel)]="planId" required>
                <option [ngValue]="null">Selecciona el plan</option>
                @for (plan of plans(); track plan.id) { <option [ngValue]="plan.id">{{ plan.nombre }} · {{ plan.precio | currency:'ARS':'symbol':'1.0-2' }}</option> }
              </select></label>
              <button class="button button-dark" type="submit" [disabled]="busy() || !memberId || !planId">Confirmar cobro en efectivo</button>
            </form>
          }
        </section>
      }

      @if (activeSection() === 'pases') {
        <section class="history-panel">
          <span class="eyebrow">ACCESO POR EL DÍA</span><h2>Registrar Pase Diario</h2>
          <form class="editor-panel" (ngSubmit)="registerPass()">
            <label>Nombre y apellido<input name="passName" [(ngModel)]="passName" required maxlength="200" /></label>
            <label>DNI<input name="passDni" [(ngModel)]="passDni" pattern="\\d{7,12}" inputmode="numeric" maxlength="12" required /></label>
            <label>Método de pago<select name="passPayment" [(ngModel)]="passMethod"><option value="Efectivo">Efectivo</option><option value="Transferencia">Transferencia</option><option value="MercadoPago">Mercado Pago</option></select></label>
            <p><strong>Monto: {{ 5000 | currency:'ARS':'symbol':'1.0-0' }}</strong></p>
            <button class="button button-dark" type="submit" [disabled]="busy()">Registrar pase diario</button>
          </form>
        </section>
      }

      @if (activeSection() === 'ventas') {
        <section class="history-panel">
          <span class="eyebrow">PUNTO DE VENTA</span><h2>Registrar venta</h2>
          @if (!products().length) { <p class="muted">Todavía no hay productos en el catálogo.</p> }
          <form class="editor-panel" (ngSubmit)="sellProduct()">
            <label>Producto<select name="product" [(ngModel)]="productId" required>
              <option [ngValue]="null">Selecciona un producto</option>
              @for (product of products(); track product.id) { <option [ngValue]="product.id" [disabled]="product.stockActual < 1">{{ product.nombre }} · {{ product.precio | currency:'ARS':'symbol':'1.0-2' }} · stock {{ product.stockActual }}</option> }
            </select></label>
            <label>Cantidad<input type="number" name="saleQuantity" min="1" [max]="selectedProduct()?.stockActual ?? 0" [(ngModel)]="saleQuantity" required /></label>
            <label>Método de pago<select name="saleMethod" [(ngModel)]="saleMethod"><option value="Efectivo">Efectivo</option><option value="Transferencia">Transferencia</option><option value="MercadoPago">Mercado Pago</option></select></label>
            @if (selectedProduct(); as product) {
              @if (saleQuantity > product.stockActual) { <p class="form-error">Stock disponible: {{ product.stockActual }}.</p> }
            }
            <button class="button button-dark" type="submit" [disabled]="busy() || !selectedProduct() || saleQuantity < 1 || saleQuantity > (selectedProduct()?.stockActual ?? 0)">Registrar venta</button>
          </form>
          <h3>Productos disponibles</h3>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Producto</th><th>Precio</th><th>Stock</th><th>Estado</th></tr></thead><tbody>
            @for (product of products(); track product.id) {
              <tr><td>{{ product.nombre }}</td><td>{{ product.precio | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ product.stockActual }}</td><td>{{ product.stockActual <= product.stockMinimo ? 'Stock bajo' : 'Disponible' }}</td></tr>
            } @empty { <tr><td colspan="4" class="table-empty">No hay productos cargados.</td></tr> }
          </tbody></table></div>
        </section>
      }

      @if (activeSection() === 'stock' && auth.isAdmin()) {
        <section class="history-panel">
          <span class="eyebrow">INVENTARIO</span><h2>Productos y stock</h2>
          <form class="editor-panel" (ngSubmit)="createProduct()">
            <div class="form-row">
              <label>Nombre<input name="newProductName" [(ngModel)]="newProduct.nombre" required maxlength="200" /></label>
              <label>Código<input name="newProductCode" [(ngModel)]="newProduct.codigo" maxlength="100" /></label>
            </div>
            <div class="form-row">
              <label>Precio<input name="newProductPrice" type="number" min="0.01" step="0.01" [(ngModel)]="newProduct.precio" required /></label>
              <label>Stock actual<input name="newProductStock" type="number" min="0" [(ngModel)]="newProduct.stockActual" required /></label>
              <label>Stock mínimo<input name="newProductMinimum" type="number" min="0" [(ngModel)]="newProduct.stockMinimo" required /></label>
            </div>
            <button class="button button-dark" type="submit" [disabled]="busy()">Crear producto</button>
          </form>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Producto</th><th>Stock actual</th><th>Mínimo</th><th>Estado</th><th>Acción</th></tr></thead><tbody>
            @for (product of products(); track product.id) {
              <tr><td>{{ product.nombre }}</td><td>{{ product.stockActual }}</td><td>{{ product.stockMinimo }}</td><td>{{ product.stockActual <= product.stockMinimo ? 'Stock bajo' : 'Disponible' }}</td><td><button class="text-button" type="button" (click)="adjustStock(product)">Ajustar</button></td></tr>
            } @empty { <tr><td colspan="5" class="table-empty">No hay productos cargados.</td></tr> }
          </tbody></table></div>
        </section>
      }

      @if (activeSection() === 'historial' && auth.isAdmin()) {
        <section class="history-panel">
          <div class="heading-split">
            <div><span class="eyebrow">TURNOS CERRADOS</span><h2>Historial de caja</h2></div>
            <div class="form-row"><label>Desde<input type="date" name="historyFrom" [(ngModel)]="historyFrom" /></label><label>Hasta<input type="date" name="historyTo" [(ngModel)]="historyTo" /></label><button class="button button-quiet" type="button" (click)="loadHistory()">Filtrar</button></div>
          </div>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Fecha</th><th>Turno</th><th>Responsable</th><th>Monto inicial</th><th>Detalle</th></tr></thead><tbody>
            @for (item of history(); track item.id) {
              <tr><td>{{ item.fecha | date:'d MMM y' }}</td><td>{{ item.turno }}</td><td>{{ item.responsableTurno }}</td><td>{{ item.montoInicial | currency:'ARS':'symbol':'1.0-2' }}</td><td><button class="text-button" type="button" (click)="viewHistoryDetail(item)">Ver movimientos</button></td></tr>
            } @empty { <tr><td colspan="5" class="table-empty">No hay turnos cerrados en el período seleccionado.</td></tr> }
          </tbody></table></div>
          @if (historyDetail(); as detail) {
            <h3>{{ detail.caja.fecha | date:'d MMM y' }} · Turno {{ detail.caja.turno }} · {{ detail.caja.responsableTurno }}</h3>
            <h4>Ventas</h4>
            <div class="table-wrap"><table class="data-table"><thead><tr><th>Fecha</th><th>Detalle</th><th>Pago</th><th>Total</th></tr></thead><tbody>
              @for (sale of detail.ventas; track sale.id) { <tr><td>{{ sale.fecha | date:'short' }}</td><td>@for (item of sale.items; track $index) { <span>{{ item.cantidad }} × {{ item.producto }}@if (!$last) {, }</span> }</td><td>{{ sale.metodoPago }}</td><td>{{ sale.montoTotal | currency:'ARS':'symbol':'1.0-2' }}</td></tr> }
              @empty { <tr><td colspan="4" class="table-empty">No hubo ventas en este turno.</td></tr> }
            </tbody></table></div>
            <h4>Pases diarios</h4>
            <div class="table-wrap"><table class="data-table"><thead><tr><th>Persona</th><th>DNI</th><th>Fecha</th><th>Monto</th><th>Pago</th></tr></thead><tbody>
              @for (pass of detail.pases; track pass.id) { <tr><td>{{ pass.nombreCompleto }}</td><td>{{ pass.dni }}</td><td>{{ pass.fecha | date:'short' }}</td><td>{{ pass.monto | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ pass.metodoPago }}</td></tr> }
              @empty { <tr><td colspan="5" class="table-empty">No hubo pases diarios en este turno.</td></tr> }
            </tbody></table></div>
          }
        </section>
      }
    </section>
  `,
})
export class OperacionesComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  readonly activeSection = signal<SeccionOperacion>('pases');
  readonly cajaActual = signal<CajaActual | null>(null);
  readonly products = signal<Producto[]>([]);
  readonly members = signal<Socio[]>([]);
  readonly plans = signal<PlanMembresia[]>([]);
  readonly history = signal<CajaTurno[]>([]);
  readonly historyDetail = signal<CajaDetalle | null>(null);
  readonly error = signal('');
  readonly notice = signal('');
  readonly busy = signal(false);
  openingAmount = 0;
  responsableTurno = '';
  cashNotes = '';
  passName = '';
  passDni = '';
  passMethod = 'Efectivo';
  memberSearchDni = '';
  memberId = '';
  planId: number | null = null;
  membershipGrantType: TipoOtorgamiento = 'Bonificada';
  historyFrom = '';
  historyTo = '';
  productId: number | null = null;
  saleQuantity = 1;
  saleMethod = 'Efectivo';
  newProduct = { nombre: '', codigo: '', precio: 0, stockActual: 0, stockMinimo: 0 };
  newMember = {
    nombre: '',
    apellido: '',
    dni: '',
    email: '',
    password: '',
    planMembresiaId: null as number | null,
    tipoOtorgamiento: 'Bonificada' as TipoOtorgamiento,
  };

  ngOnInit(): void {
    if (this.auth.isAdmin()) {
      this.activeSection.set('caja');
      this.loadCaja();
    }
    this.loadProducts();
    this.http.get<PlanMembresia[]>(`${API_BASE_URL}/membresias/planes`).subscribe({
      next: plans => this.plans.set(plans),
      error: error => this.setError(error),
    });
    if (this.auth.isAdmin()) this.loadHistory();
  }

  setSection(section: SeccionOperacion): void {
    this.activeSection.set(section);
    this.error.set('');
    this.notice.set('');
    if (section === 'caja' && this.auth.isAdmin()) this.loadCaja();
    if (section === 'historial' && this.auth.isAdmin()) this.loadHistory();
    if (section === 'ventas' || section === 'stock') this.loadProducts();
  }

  selectedProduct(): Producto | null {
    return this.products().find(product => product.id === this.productId) ?? null;
  }

  loadCaja(): void {
    this.http.get<CajaActual>(`${API_BASE_URL}/caja/actual`).subscribe({
      next: caja => this.cajaActual.set(caja),
      error: error => this.setError(error),
    });
  }

  loadProducts(): void {
    this.http.get<Producto[]>(`${API_BASE_URL}/productos`).subscribe({
      next: products => this.products.set(products),
      error: error => this.setError(error),
    });
  }

  openCash(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/caja/abrir`, {
      responsableTurno: this.responsableTurno,
      montoInicial: this.openingAmount,
    }), 'Caja abierta correctamente.', () => {
      this.responsableTurno = '';
      this.loadCaja();
    });
  }

  closeCash(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/caja/cerrar`, { observaciones: this.cashNotes }),
      'Caja cerrada correctamente.', () => {
        this.cashNotes = '';
        this.loadCaja();
        this.loadHistory();
      });
  }

  findMember(): void {
    const params = new HttpParams().set('buscar', this.memberSearchDni.trim()).set('page', 1).set('pageSize', 50);
    this.http.get<{ items: Socio[] }>(`${API_BASE_URL}/admin/usuarios`, { params }).subscribe({
      next: response => {
        const matches = response.items.filter(member => member.dni === this.memberSearchDni.trim());
        this.members.set(matches);
        this.memberId = matches.length === 1 ? matches[0].id : '';
        if (matches.length === 0) this.error.set('No se encontró un socio con ese DNI.');
        else this.error.set('');
      },
      error: error => this.setError(error),
    });
  }

  assignMembership(): void {
    if (!this.memberId || !this.planId) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/admin/membresias/asignar`, {
      usuarioId: this.memberId,
      planMembresiaId: this.planId,
      tipoOtorgamiento: this.membershipGrantType,
    }), 'Membresía asignada correctamente.', () => this.clearMemberSearch());
  }

  createMemberWithPlan(): void {
    if (!this.newMember.planMembresiaId) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/admin/usuarios/crear-con-membresia`, this.newMember),
      'Socio registrado y membresía asignada correctamente.', () => {
        this.newMember = {
          nombre: '', apellido: '', dni: '', email: '', password: '',
          planMembresiaId: null, tipoOtorgamiento: 'Bonificada',
        };
      });
  }

  registerCashPayment(): void {
    if (!this.memberId || !this.planId) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/pagos/efectivo`, {
      usuarioId: this.memberId,
      planMembresiaId: this.planId,
      metodoPago: 'Efectivo',
    }), 'Cobro en efectivo registrado y membresía activada.', () => {
      this.clearMemberSearch();
      if (this.auth.isAdmin()) this.loadCaja();
    });
  }

  registerPass(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/pases-diarios`, {
      nombreCompleto: this.passName,
      dni: this.passDni,
      metodoPago: this.passMethod,
    }), 'Pase diario registrado correctamente.', () => {
      this.passName = '';
      this.passDni = '';
      if (this.auth.isAdmin()) this.loadCaja();
    });
  }

  sellProduct(): void {
    const product = this.selectedProduct();
    if (!this.productId || !product || this.saleQuantity > product.stockActual) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/productos/ventas`, {
      metodoPago: this.saleMethod,
      items: [{ productoId: this.productId, cantidad: this.saleQuantity }],
    }), 'Venta registrada y stock actualizado.', () => {
      this.loadProducts();
      if (this.auth.isAdmin()) this.loadCaja();
    });
  }

  createProduct(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/productos`, this.newProduct), 'Producto agregado.', () => {
      this.newProduct = { nombre: '', codigo: '', precio: 0, stockActual: 0, stockMinimo: 0 };
      this.loadProducts();
    });
  }

  adjustStock(product: Producto): void {
    const value = window.prompt(`Nuevo stock para ${product.nombre}:`, product.stockActual.toString());
    if (value === null) return;
    const stock = Number(value);
    if (!Number.isInteger(stock) || stock < 0) {
      this.error.set('El stock debe ser un entero igual o mayor que cero.');
      return;
    }
    this.runRequest(this.http.put(`${API_BASE_URL}/productos/${product.id}/stock`, { stockActual: stock }),
      'Stock actualizado.', () => this.loadProducts());
  }

  loadHistory(): void {
    let params = new HttpParams();
    if (this.historyFrom) params = params.set('desde', this.historyFrom);
    if (this.historyTo) params = params.set('hasta', this.historyTo);
    this.http.get<CajaTurno[]>(`${API_BASE_URL}/caja/historial`, { params }).subscribe({
      next: value => this.history.set(value),
      error: error => this.setError(error),
    });
  }

  viewHistoryDetail(caja: CajaTurno): void {
    this.historyDetail.set(null);
    this.http.get<CajaDetalle>(`${API_BASE_URL}/caja/${caja.id}/detalle`).subscribe({
      next: detail => this.historyDetail.set(detail),
      error: error => this.setError(error),
    });
  }

  private clearMemberSearch(): void {
    this.memberId = '';
    this.planId = null;
    this.members.set([]);
    this.memberSearchDni = '';
  }

  private runRequest(request: Observable<unknown>, message: string, success: () => void): void {
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    request.subscribe({
      next: () => { this.notice.set(message); success(); this.busy.set(false); },
      error: (error: HttpErrorResponse) => { this.setError(error); this.busy.set(false); },
    });
  }

  private setError(error: HttpErrorResponse): void {
    this.error.set(error.error?.message ?? 'No se pudo completar la operación.');
  }
}
