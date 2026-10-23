import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { AuthService } from '../../core/auth.service';

interface CajaTurno {
  id: number;
  fecha: string;
  turno: string;
  responsableTurno: string;
  montoInicial: number;
  montoFinalEfectivoEsperado: number;
  totalCobrosEfectivo: number;
  totalCobrosDigitales: number;
  estado: string;
}

interface Producto {
  id: number;
  nombre: string;
  stockActual: number;
  stockMinimo: number;
  precio: number;
  codigo: string | null;
}

interface CajaActual {
  fecha: string;
  turno: string;
  caja: CajaTurno | null;
}

interface SocioItem {
  id: string;
  nombre: string;
  apellido: string;
  dni: string;
}

interface PlanMembresia {
  id: number;
  nombre: string;
  precio: number;
}

interface CajaHistorial extends CajaTurno {
  montoFinalEfectivoReal: number | null;
  diferencia: number | null;
  totalCobrosDigitales: number;
}

@Component({
  selector: 'app-operaciones',
  imports: [FormsModule, CurrencyPipe, DatePipe],
  template: `
    <section class="content-page">
      <div class="page-heading"><span class="eyebrow">OPERACIÓN DEL GIMNASIO</span><h1>Operaciones</h1><p class="lede">Caja, productos y accesos diarios.</p></div>
      @if (notice()) { <div class="routine-assigned-toast" role="status">{{ notice() }}</div> }
      @if (error()) { <p class="form-error" role="alert">{{ error() }}</p> }

      @if (auth.isAdmin()) {
        <section class="history-panel">
          <div class="heading-split"><div><span class="eyebrow">ADMINISTRACIÓN · CAJA</span><h2>{{ cajaActual()?.turno ?? 'Turno actual' }} · {{ cajaActual()?.fecha | date:'d MMM y' }}</h2></div>
            <span class="status-badge" [class.status-active]="cajaActual()?.caja" [class.status-inactive]="!cajaActual()?.caja">{{ cajaActual()?.caja ? 'ABIERTA' : 'CERRADA' }}</span>
          </div>
          @if (cajaActual()?.caja; as caja) {
            <p class="lede">Responsable del turno: <strong>{{ caja.responsableTurno }}</strong></p>
            <div class="profile-grid">
              <div class="detail-line"><span>Saldo inicial</span><strong>{{ caja.montoInicial | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Cobros en efectivo</span><strong>{{ caja.totalCobrosEfectivo | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Cobros digitales</span><strong>{{ caja.totalCobrosDigitales | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Efectivo esperado</span><strong>{{ caja.montoFinalEfectivoEsperado | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
            </div>
            <form class="editor-panel" (ngSubmit)="closeCash()">
              <h3>Arqueo de cierre</h3>
              <label>Efectivo real contado<input type="number" name="cashReal" min="0" step="0.01" [(ngModel)]="cashReal" required /></label>
              <label>Observaciones<textarea name="cashNotes" [(ngModel)]="cashNotes" rows="2" maxlength="2000"></textarea></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Cerrar turno</button>
            </form>
          } @else {
            <form class="editor-panel" (ngSubmit)="openCash()">
              <h3>Apertura de caja</h3>
              <p>Turno asignado: <strong>{{ cajaActual()?.turno }}</strong>. Cada turno tiene su propia apertura y cierre; la misma persona puede cubrir más de un turno.</p>
              <label>Persona responsable del turno<input name="cashResponsible" [(ngModel)]="responsableTurno" maxlength="150" minlength="2" autocomplete="name" required /></label>
              <label>Monto inicial<input type="number" name="cashOpening" min="0" step="0.01" [(ngModel)]="openingAmount" required /></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Abrir caja</button>
            </form>
          }
        </section>
      }

      <div class="profile-grid">
        <section class="history-panel">
          <span class="eyebrow">COBRO DE MEMBRESÍA · EFECTIVO</span><h2>Registrar cobro</h2>
          <form class="editor-panel" (ngSubmit)="findMember()">
            <label>DNI del socio<input name="paymentDni" [(ngModel)]="memberSearchDni" pattern="\\d{7,12}" inputmode="numeric" maxlength="12" required /></label>
            <button class="button button-quiet" type="submit" [disabled]="busy()">Buscar socio</button>
          </form>
          @if (members().length) {
            <label>Socio<select name="selectedMember" [(ngModel)]="memberId" required>
              <option value="">Selecciona un socio</option>
              @for (member of members(); track member.id) { <option [value]="member.id">{{ member.nombre }} {{ member.apellido }} · DNI {{ member.dni }}</option> }
            </select></label>
            <label>Plan<select name="selectedPlan" [(ngModel)]="planId" required>
              <option [ngValue]="null">Selecciona un plan</option>
              @for (plan of plans(); track plan.id) { <option [ngValue]="plan.id">{{ plan.nombre }} · {{ plan.precio | currency:'ARS':'symbol':'1.0-2' }}</option> }
            </select></label>
            <button class="button button-dark" type="button" [disabled]="busy() || !memberId || !planId" (click)="registerCashPayment()">Confirmar cobro en efectivo</button>
          }
        </section>
        <section class="history-panel">
          <span class="eyebrow">ACCESO · $5000</span><h2>Registrar Pase Diario</h2>
          <form class="editor-panel" (ngSubmit)="registerPass()">
            <label>Nombre y apellido<input name="passName" [(ngModel)]="passName" required maxlength="200" /></label>
            <label>DNI<input name="passDni" [(ngModel)]="passDni" required pattern="\\d{7,12}" inputmode="numeric" maxlength="12" /></label>
            <label>Método de pago<select name="passPayment" [(ngModel)]="passMethod"><option value="Efectivo">Efectivo</option><option value="Transferencia">Transferencia</option><option value="MercadoPago">Mercado Pago</option></select></label>
            <p><strong>Monto: {{ 5000 | currency:'ARS':'symbol':'1.0-0' }}</strong></p>
            <button class="button button-dark" type="submit" [disabled]="busy()">Confirmar acceso por hoy</button>
          </form>
        </section>

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
              @if (saleQuantity > product.stockActual) { <p class="form-error">Stock disponible: {{ product.stockActual }}. Reduce la cantidad.</p> }
            }
            <button class="button button-dark" type="submit" [disabled]="busy() || !selectedProduct() || saleQuantity < 1 || saleQuantity > (selectedProduct()?.stockActual ?? 0)">Registrar venta</button>
          </form>
        </section>
      </div>

      @if (auth.isAdmin()) {
        <section class="history-panel">
          <span class="eyebrow">CATÁLOGO · ADMINISTRACIÓN</span><h2>Agregar producto</h2>
          <form class="editor-panel" (ngSubmit)="createProduct()">
            <div class="form-row">
              <label>Nombre<input name="newProductName" [(ngModel)]="newProduct.nombre" required maxlength="200" /></label>
              <label>Código<input name="newProductCode" [(ngModel)]="newProduct.codigo" maxlength="100" /></label>
            </div>
            <div class="form-row">
              <label>Precio<input name="newProductPrice" type="number" min="0.01" step="0.01" [(ngModel)]="newProduct.precio" required /></label>
              <label>Stock inicial<input name="newProductStock" type="number" min="0" step="1" [(ngModel)]="newProduct.stockActual" required /></label>
              <label>Stock mínimo<input name="newProductMinimum" type="number" min="0" step="1" [(ngModel)]="newProduct.stockMinimo" required /></label>
            </div>
            <button class="button button-dark" type="submit" [disabled]="busy()">Crear producto</button>
          </form>
        </section>
      }

      <section class="history-panel">
        <span class="eyebrow">INVENTARIO</span><h2>Productos disponibles</h2>
        <div class="table-wrap"><table class="data-table"><thead><tr><th>Producto</th><th>Precio</th><th>Stock</th><th>Estado</th>@if (auth.isAdmin()) { <th>Acciones</th> }</tr></thead><tbody>
          @for (product of products(); track product.id) {
            <tr><td>{{ product.nombre }} @if (product.codigo) { <small>{{ product.codigo }}</small> }</td><td>{{ product.precio | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ product.stockActual }}</td>
              <td><span class="status-badge" [class.status-active]="product.stockActual > product.stockMinimo" [class.status-inactive]="product.stockActual <= product.stockMinimo">{{ product.stockActual <= product.stockMinimo ? 'STOCK BAJO' : 'DISPONIBLE' }}</span></td>
              @if (auth.isAdmin()) { <td><button class="text-button" type="button" (click)="adjustStock(product)">Ajustar stock</button></td> }
            </tr>
          } @empty { <tr><td class="table-empty" [attr.colspan]="auth.isAdmin() ? 5 : 4">No hay productos cargados.</td></tr> }
        </tbody></table></div>
      </section>

      @if (auth.isAdmin()) {
        <section class="history-panel">
          <div class="heading-split"><div><span class="eyebrow">REPORTE</span><h2>Historial de caja</h2></div>
            <div class="form-row"><label>Desde<input type="date" name="historyFrom" [(ngModel)]="historyFrom" /></label><label>Hasta<input type="date" name="historyTo" [(ngModel)]="historyTo" /></label><button class="button button-quiet" type="button" (click)="loadHistory()">Filtrar</button></div>
          </div>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Fecha / turno</th><th>Responsable</th><th>Efectivo esperado</th><th>Efectivo real</th><th>Diferencia</th><th>Digital</th></tr></thead><tbody>
            @for (item of history(); track item.id) { <tr><td>{{ item.fecha | date:'d MMM y' }} · {{ item.turno }}</td><td>{{ item.responsableTurno }}</td><td>{{ item.montoFinalEfectivoEsperado | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ item.montoFinalEfectivoReal | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ item.diferencia | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ item.totalCobrosDigitales | currency:'ARS':'symbol':'1.0-2' }}</td></tr> }
            @empty { <tr><td class="table-empty" colspan="6">No hay turnos cerrados en el período seleccionado.</td></tr> }
          </tbody></table></div>
        </section>
      }
    </section>
  `,
})
export class OperacionesComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  readonly cajaActual = signal<CajaActual | null>(null);
  readonly products = signal<Producto[]>([]);
  readonly members = signal<SocioItem[]>([]);
  readonly plans = signal<PlanMembresia[]>([]);
  readonly history = signal<CajaHistorial[]>([]);
  readonly error = signal('');
  readonly notice = signal('');
  readonly busy = signal(false);
  openingAmount = 0;
  responsableTurno = '';
  cashReal = 0;
  cashNotes = '';
  passName = '';
  passDni = '';
  passMethod = 'Efectivo';
  memberSearchDni = '';
  memberId = '';
  planId: number | null = null;
  historyFrom = '';
  historyTo = '';
  productId: number | null = null;
  saleQuantity = 1;
  saleMethod = 'Efectivo';
  newProduct = { nombre: '', codigo: '', precio: 0, stockActual: 0, stockMinimo: 0 };

  ngOnInit(): void {
    this.loadCaja();
    this.loadProducts();
    this.http.get<PlanMembresia[]>(`${API_BASE_URL}/membresias/planes`).subscribe({
      next: plans => this.plans.set(plans),
      error: error => this.setError(error),
    });
    if (this.auth.isAdmin()) this.loadHistory();
  }

  selectedProduct(): Producto | null {
    return this.products().find(product => product.id === this.productId) ?? null;
  }

  loadCaja(): void {
    this.http.get<CajaActual>(`${API_BASE_URL}/caja/actual`).subscribe({
      next: caja => this.cajaActual.set(caja),
      error: error => { if (error.status !== 403) this.setError(error); },
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
    }), 'Caja abierta y responsable asignado correctamente.', () => {
      this.responsableTurno = '';
      this.loadCaja();
    });
  }

  closeCash(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/caja/cerrar`, {
      montoFinalEfectivoReal: this.cashReal,
      observaciones: this.cashNotes,
    }), 'Caja cerrada correctamente.', () => { this.loadCaja(); this.loadHistory(); this.cashNotes = ''; });
  }

  registerPass(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/pases-diarios`, {
      nombreCompleto: this.passName,
      dni: this.passDni,
      metodoPago: this.passMethod,
    }), 'Pase diario confirmado: acceso habilitado por hoy.', () => {
      this.passName = '';
      this.passDni = '';
    });
  }

  sellProduct(): void {
    if (!this.productId || !this.selectedProduct() || this.saleQuantity > this.selectedProduct()!.stockActual) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/productos/ventas`, {
      metodoPago: this.saleMethod,
      items: [{ productoId: this.productId, cantidad: this.saleQuantity }],
    }), 'Venta registrada y stock actualizado.', () => this.loadProducts());
  }

  createProduct(): void {
    this.runRequest(this.http.post(`${API_BASE_URL}/productos`, this.newProduct), 'Producto agregado.', () => {
      this.newProduct = { nombre: '', codigo: '', precio: 0, stockActual: 0, stockMinimo: 0 };
      this.loadProducts();
    });
  }

  findMember(): void {
    const params = { buscar: this.memberSearchDni.trim(), page: '1', pageSize: '50' };
    this.http.get<{ items: SocioItem[] }>(`${API_BASE_URL}/admin/usuarios`, { params }).subscribe({
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

  registerCashPayment(): void {
    if (!this.memberId || !this.planId) return;
    this.runRequest(this.http.post(`${API_BASE_URL}/pagos/efectivo`, {
      usuarioId: this.memberId,
      planMembresiaId: this.planId,
      metodoPago: 'Efectivo',
    }), 'Cobro en efectivo registrado y membresía activada.', () => {
      this.memberId = '';
      this.planId = null;
      this.members.set([]);
      this.memberSearchDni = '';
      this.loadCaja();
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
    this.runRequest(this.http.put(`${API_BASE_URL}/productos/${product.id}/stock`, { stockActual: stock }), 'Stock actualizado.', () => this.loadProducts());
  }

  loadHistory(): void {
    let params = new URLSearchParams();
    if (this.historyFrom) params.set('desde', this.historyFrom);
    if (this.historyTo) params.set('hasta', this.historyTo);
    this.http.get<CajaHistorial[]>(`${API_BASE_URL}/caja/historial?${params}`).subscribe({
      next: value => this.history.set(value),
      error: error => this.setError(error),
    });
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
