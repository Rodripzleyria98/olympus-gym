import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from '../../core/api.config';

export interface PreferenciaPago {
  pagoId: string;
  preferenceId: string;
  initPoint: string;
  sandboxInitPoint: string;
}

export interface EstadoPago {
  pagoId: string;
  planNombre: string;
  monto: number;
  estadoPago: string;
}

@Injectable({ providedIn: 'root' })
export class PagosService {
  private readonly http = inject(HttpClient);

  iniciarRenovacion(planMembresiaId: number) {
    return this.http.post<PreferenciaPago>(`${API_BASE_URL}/pagos/crear-preferencia`, { planMembresiaId });
  }

  obtenerEstado(pagoId: string) {
    return this.http.get<EstadoPago>(`${API_BASE_URL}/pagos/${encodeURIComponent(pagoId)}`);
  }
}