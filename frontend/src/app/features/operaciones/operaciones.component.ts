import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { AuthService } from '../../core/auth.service';

type SeccionOperacion = 'caja' | 'socios' | 'cobros' | 'pases' | 'ventas' | 'stock' | 'historial';
type MetodoOtorgamiento = 'Efectivo' | 'Transferencia' | 'MercadoPago';
interface CajaTurno {
  id: number;
  fecha: string;
  turno: string;
  responsableTurno: string;
  usuarioAperturaId: string;
  usuarioCierreId: string | null;
  fechaHoraApertura: string | null;
  fechaHoraCierre: string | null;
  montoInicial: number;
  montoFinalEfectivoEsperado: number;
  montoFinalEfectivoReal: number | null;
  diferencia: number | null;
  totalCobrosEfectivo: number;
  totalCobrosDigitales: number;
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
  totalesTurno: CajaDiaTotales;
  totalesDia: CajaDiaTotales;
}

interface CajaDiaTotales {
  fecha: string;
  efectivo: number;
  transferencias: number;
  otros: number;
  total: number;
}

interface CajaActual {
  fecha: string;
  turno: string;
  detalle: CajaDetalle | null;
}

interface Producto {
  id: number;
  nombre: string;
  sabor: string | null;
  descripcion: string | null;
  categoria: string;
  stockActual: number;
  stockMinimo: number;
  precio: number;
  codigo: string | null;
  isActivo: boolean;
}

interface SocioCreado {
  usuarioId: string;
  nombreCompleto: string;
  dni: string;
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
          <button class="button button-small" [class.button-accent]="activeSection() === 'historial'" type="button" (click)="setSection('historial')">Historial de caja</button>
          <button class="button button-small" [class.button-accent]="activeSection() === 'stock'" type="button" (click)="setSection('stock')">Inventario</button>
        }
        @if (auth.isAdmin() || auth.isProfesor()) {
          <button class="button button-small" [class.button-accent]="activeSection() === 'socios'" type="button" (click)="setSection('socios')">Socios y membresías</button>
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
              <div class="detail-line"><span>Usuario que abrió</span><strong>{{ detail.caja.usuarioAperturaId }}</strong></div>
              <div class="detail-line"><span>Apertura</span><strong>{{ detail.caja.fechaHoraApertura | date:'short' }}</strong></div>
              <div class="detail-line"><span>Monto de apertura</span><strong>{{ detail.caja.montoInicial | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
            </div>
            <h3>Acumulado del día</h3>
            <div class="profile-grid">
              <div class="detail-line"><span>Efectivo</span><strong>{{ detail.totalesDia.efectivo | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Transferencias</span><strong>{{ detail.totalesDia.transferencias | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Otros medios</span><strong>{{ detail.totalesDia.otros | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Total del día</span><strong>{{ detail.totalesDia.total | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
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
              <p>Efectivo esperado en este turno: <strong>{{ detail.caja.montoInicial + detail.totalesTurno.efectivo | currency:'ARS':'symbol':'1.0-2' }}</strong></p>
              <label>Efectivo contado<input type="number" name="cashClosingAmount" min="0" step="0.01" [(ngModel)]="cashClosingAmount" /></label>
              <label>Observaciones (opcional)<textarea name="cashNotes" [(ngModel)]="cashNotes" rows="2" maxlength="2000"></textarea></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Cerrar turno</button>
            </form>
          } @else {
            <form class="editor-panel" (ngSubmit)="openCash()">
              <h3>Abrir caja · {{ cajaActual()?.turno }}</h3>
              <label>Turno<select name="cashTurno" [(ngModel)]="cashTurno" required>
                <option value="Mañana">Mañana · 07:00 a 15:00</option>
                <option value="Tarde">Tarde · 15:00 a 19:00</option>
                <option value="Noche">Noche · 19:00 a 22:00</option>
              </select></label>
              <label>Persona responsable<input name="cashResponsible" [(ngModel)]="responsableTurno" maxlength="150" minlength="2" autocomplete="name" required /></label>
              <label>Monto inicial de caja<input type="number" name="cashOpening" min="0" step="0.01" [(ngModel)]="openingAmount" required /></label>
              <button class="button button-dark" type="submit" [disabled]="busy()">Abrir caja</button>
            </form>
          }
          @if (closedShiftDetail(); as closed) {
            <h3>Resumen del turno cerrado · {{ closed.caja.fechaHoraCierre | date:'short' }}</h3>
            <p>Usuario que cerró: {{ closed.caja.usuarioCierreId }}</p>
            <p>Efectivo esperado: {{ closed.caja.montoFinalEfectivoEsperado | currency:'ARS':'symbol':'1.0-2' }}
              @if (closed.caja.diferencia !== null) { · Diferencia: {{ closed.caja.diferencia | currency:'ARS':'symbol':'1.0-2' }} }
            </p>
            <div class="profile-grid">
              <div class="detail-line"><span>Efectivo del día</span><strong>{{ closed.totalesDia.efectivo | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Transferencias del día</span><strong>{{ closed.totalesDia.transferencias | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Otros medios del día</span><strong>{{ closed.totalesDia.otros | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Total del día</span><strong>{{ closed.totalesDia.total | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
            </div>
          }
        </section>
      }

      @if (activeSection() === 'socios' && (auth.isAdmin() || auth.isProfesor())) {
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
              <label>Fecha de inicio<input type="date" name="newMemberStartDate" [(ngModel)]="newMember.fechaInicio" required /></label>
              <label>Método de cobro<select name="newMemberGrantType" [(ngModel)]="newMember.tipoOtorgamiento">
                <option value="Efectivo">Cobrar en efectivo</option>
                <option value="Transferencia">Transferencia</option>
                <option value="MercadoPago">Mercado Pago (Débito / Crédito)</option>
              </select></label>
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
                <label>Fecha de inicio (opcional; por defecto continúa la vigencia actual)<input type="date" name="assignMembershipStartDate" [(ngModel)]="membershipStartDate" /></label>
                <label>Método de cobro<select name="assignGrantType" [(ngModel)]="membershipGrantType">
                  <option value="Efectivo">Cobrar en efectivo</option>
                  <option value="Transferencia">Transferencia</option>
                  <option value="MercadoPago">Mercado Pago (Débito / Crédito)</option>
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
          <h3>{{ editingProductId() !== null ? 'Editar producto' : 'Nuevo producto' }}</h3>
          <form class="editor-panel" (ngSubmit)="saveProduct()">
            <div class="form-row">
              <label>Nombre<input name="productName" [(ngModel)]="newProduct.nombre" required maxlength="200" /></label>
              <label>Sabor<input name="productFlavor" [(ngModel)]="newProduct.sabor" maxlength="100" /></label>
              <label>Categoría<input name="productCategory" [(ngModel)]="newProduct.categoria" required maxlength="100" /></label>
              <label>Código<input name="productCode" [(ngModel)]="newProduct.codigo" maxlength="100" /></label>
            </div>
            <label>Descripción<textarea name="productDescription" [(ngModel)]="newProduct.descripcion" rows="2" maxlength="1000"></textarea></label>
            <div class="form-row">
              <label>Precio<input name="productPrice" type="number" min="0.01" step="0.01" [(ngModel)]="newProduct.precio" required /></label>
              <label>Stock actual<input name="productStock" type="number" min="0" [(ngModel)]="newProduct.stockActual" required /></label>
              <label>Stock mínimo<input name="productMinimum" type="number" min="0" [(ngModel)]="newProduct.stockMinimo" required /></label>
            </div>
            <button class="button button-dark" type="submit" [disabled]="busy()">{{ editingProductId() !== null ? 'Guardar cambios' : 'Crear producto' }}</button>
            @if (editingProductId() !== null) { <button class="button button-quiet" type="button" (click)="cancelEditProduct()">Cancelar</button> }
          </form>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Producto</th><th>Sabor / categoría</th><th>Precio</th><th>Stock actual</th><th>Mínimo</th><th>Estado</th><th>Acción</th></tr></thead><tbody>
            @for (product of products(); track product.id) {
              <tr><td>{{ product.nombre }}</td><td>{{ product.sabor || '—' }} · {{ product.categoria }}</td><td>{{ product.precio | currency:'ARS':'symbol':'1.0-2' }}</td><td>{{ product.stockActual }}</td><td>{{ product.stockMinimo }}</td><td>{{ product.stockActual <= product.stockMinimo ? 'Stock bajo' : 'Disponible' }}</td><td><select class="product-actions" [attr.aria-label]="'Acciones para ' + product.nombre" (change)="handleProductAction(product, $event)"><option value="">Acciones…</option><option value="edit">Editar</option><option value="stock">Ajustar stock</option><option value="delete">Eliminar</option></select></td></tr>
            } @empty { <tr><td colspan="7" class="table-empty">No hay productos cargados.</td></tr> }
          </tbody></table></div>
        </section>
      }

      @if (activeSection() === 'historial' && auth.isAdmin()) {
        <section class="history-panel">
          <div class="heading-split">
            <div><span class="eyebrow">TURNOS CERRADOS</span><h2>Historial de caja</h2></div>
            <div class="form-row"><label>Desde<input type="date" name="historyFrom" [(ngModel)]="historyFrom" /></label><label>Hasta<input type="date" name="historyTo" [(ngModel)]="historyTo" /></label><button class="button button-quiet" type="button" (click)="loadHistory()">Filtrar</button></div>
          </div>
          <div class="table-wrap"><table class="data-table"><thead><tr><th>Turno</th><th>Responsable</th><th>Apertura</th><th>Cierre</th><th>Monto inicial</th><th>Detalle</th></tr></thead><tbody>
            @for (item of history(); track item.id) {
              <tr><td>{{ item.turno }} · {{ item.fecha | date:'d MMM y' }}</td><td>{{ item.responsableTurno }}</td><td>{{ item.fechaHoraApertura | date:'short' }}</td><td>{{ item.fechaHoraCierre | date:'short' }}</td><td>{{ item.montoInicial | currency:'ARS':'symbol':'1.0-2' }}</td><td><button class="text-button" type="button" (click)="viewHistoryDetail(item)">Ver movimientos</button></td></tr>
            } @empty { <tr><td colspan="6" class="table-empty">No hay turnos cerrados en el período seleccionado.</td></tr> }
          </tbody></table></div>
          @if (historyDetail(); as detail) {
            <h3>{{ detail.caja.fecha | date:'d MMM y' }} · Turno {{ detail.caja.turno }} · {{ detail.caja.responsableTurno }}</h3>
            <div class="profile-grid">
              <div class="detail-line"><span>Efectivo del día</span><strong>{{ detail.totalesDia.efectivo | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Transferencias del día</span><strong>{{ detail.totalesDia.transferencias | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Otros medios del día</span><strong>{{ detail.totalesDia.otros | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
              <div class="detail-line"><span>Total acumulado del día</span><strong>{{ detail.totalesDia.total | currency:'ARS':'symbol':'1.0-2' }}</strong></div>
            </div>
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
  readonly closedShiftDetail = signal<CajaDetalle | null>(null);
  readonly editingProductId = signal<number | null>(null);
  readonly error = signal('');
  readonly notice = signal('');
  readonly busy = signal(false);
  openingAmount = 0;
  cashTurno = 'Mañana';
  cashClosingAmount: number | null = null;
  responsableTurno = '';
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
  newProduct = {
    nombre: '', sabor: '', descripcion: '', categoria: '', codigo: '',
    precio: 0, stockActual: 0, stockMinimo: 0,
  };
  membershipGrantType: MetodoOtorgamiento = 'Efectivo';
  membershipStartDate = '';
  newMember = {
    nombre: '',
    apellido: '',
    dni: '',
    email: '',
    password: '',
    planMembresiaId: null as number | null,
    fechaInicio: this.todayDate(),
    tipoOtorgamiento: 'Efectivo' as MetodoOtorgamiento,
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
      next: caja => {
        this.cajaActual.set(caja);
        if (!caja.detalle) this.cashTurno = caja.turno;
      },
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
      turno: this.cashTurno,
      montoInicial: this.openingAmount,
    }), 'Caja abierta correctamente.', () => {
      this.responsableTurno = '';
      this.closedShiftDetail.set(null);
      this.loadCaja();
    });
  }

  closeCash(): void {
    this.runRequest(this.http.post<CajaTurno>(`${API_BASE_URL}/caja/cerrar`, {
      observaciones: this.cashNotes,
      montoFinalEfectivoReal: this.cashClosingAmount,
    }), 'Caja cerrada correctamente.', caja => {
        this.cashNotes = '';
        this.cashClosingAmount = null;
        this.loadCaja();
        this.loadHistory();
        this.http.get<CajaDetalle>(`${API_BASE_URL}/caja/${caja.id}/detalle`).subscribe({
          next: detail => this.closedShiftDetail.set(detail),
          error: error => this.setError(error),
        });
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
      fechaInicio: this.membershipStartDate || null,
    }), 'Membresía asignada correctamente.', () => {
      this.clearMemberSearch();
      this.loadCaja();
    });
  }

  createMemberWithPlan(): void {
    if (!this.newMember.planMembresiaId) return;
    this.runRequest(this.http.post<SocioCreado>(`${API_BASE_URL}/admin/usuarios/crear-con-membresia`, this.newMember),
      'Socio registrado y membresía asignada correctamente.', socio => {
        this.newMember = {
          nombre: '', apellido: '', dni: '', email: '', password: '',
          planMembresiaId: null,
          fechaInicio: this.todayDate(),
          tipoOtorgamiento: 'Efectivo',
        };
        this.memberSearchDni = socio.dni;
        this.findMember();
        this.loadCaja();
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

  saveProduct(): void {
    const id = this.editingProductId();
    const request = id === null
      ? this.http.post<Producto>(`${API_BASE_URL}/productos`, this.newProduct)
      : this.http.put<Producto>(`${API_BASE_URL}/productos/${id}`, this.newProduct);
    this.runRequest(request, id === null ? 'Producto agregado.' : 'Producto actualizado.', () => {
      this.cancelEditProduct();
      this.loadProducts();
    });
  }

  editProduct(product: Producto): void {
    this.editingProductId.set(product.id);
    this.newProduct = {
      nombre: product.nombre,
      sabor: product.sabor ?? '',
      descripcion: product.descripcion ?? '',
      categoria: product.categoria,
      codigo: product.codigo ?? '',
      precio: product.precio,
      stockActual: product.stockActual,
      stockMinimo: product.stockMinimo,
    };
  }

  cancelEditProduct(): void {
    this.editingProductId.set(null);
    this.newProduct = {
      nombre: '', sabor: '', descripcion: '', categoria: '', codigo: '',
      precio: 0, stockActual: 0, stockMinimo: 0,
    };
  }

  deactivateProduct(product: Producto): void {
    if (!window.confirm(`¿Eliminar ${product.nombre} del inventario? Se desactivará y ya no aparecerá en ventas.`)) return;
    this.runRequest(this.http.delete(`${API_BASE_URL}/productos/${product.id}`),
      'Producto eliminado del inventario.', () => this.loadProducts());
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

  handleProductAction(product: Producto, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const action = select.value;
    select.value = '';
    switch (action) {
      case 'edit':
        this.editProduct(product);
        break;
      case 'stock':
        this.adjustStock(product);
        break;
      case 'delete':
        this.deactivateProduct(product);
        break;
    }
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
    this.membershipStartDate = '';
    this.members.set([]);
    this.memberSearchDni = '';
  }

  private todayDate(): string {
    const localDate = new Date(Date.now() - new Date().getTimezoneOffset() * 60_000);
    return localDate.toISOString().slice(0, 10);
  }

  private runRequest<T = unknown>(
    request: Observable<T>,
    message: string,
    success: (response: T) => void = () => undefined,
  ): void {
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    request.subscribe({
      next: response => { this.notice.set(message); success(response); this.busy.set(false); },
      error: (error: HttpErrorResponse) => { this.setError(error); this.busy.set(false); },
    });
  }

  private setError(error: HttpErrorResponse): void {
    this.error.set(error.error?.message ?? 'No se pudo completar la operación.');
  }
}
