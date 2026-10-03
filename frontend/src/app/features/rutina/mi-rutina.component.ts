import { DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { API_BASE_URL } from '../../core/api.config';

interface EjercicioRutina {
  id: string;
  orden: number;
  nombre: string;
  seriesYRepeticiones: string;
  notas: string | null;
}

interface DiaRutina {
  id: string;
  nombreDia: string;
  orden: number;
  enfoque: string;
  ejercicios: EjercicioRutina[];
}

interface Rutina {
  id: string;
  usuarioId: string;
  nombreSocio: string;
  titulo: string;
  fechaInicio: string | null;
  fechaRevision: string | null;
  isActivo: boolean;
  dias: DiaRutina[];
}

@Component({
  selector: 'app-mi-rutina',
  imports: [DatePipe],
  template: `
    <section class="routine-page">
      @if (routine(); as plan) {
        <header class="routine-header">
          <span class="eyebrow">OLYMPUS GYM <span class="routine-slash">/</span> {{ plan.nombreSocio }}</span>
          <p class="routine-title">PLAN DE ENTRENAMIENTO</p>
          <h1>{{ plan.titulo }}</h1>
          <div class="routine-meta">
            <div><span>SOCIO</span><strong>{{ plan.nombreSocio }}</strong></div>
            <div><span>FECHA DE INICIO</span><strong>{{ plan.fechaInicio ? (plan.fechaInicio | date:'d MMM y':'UTC') : 'Pendiente' }}</strong></div>
            <div><span>FECHA DE REVISIÓN</span><strong>{{ plan.fechaRevision ? (plan.fechaRevision | date:'d MMM y':'UTC') : 'Por definir' }}</strong></div>
          </div>
        </header>
        <div class="routine-grid">
          @for (day of plan.dias; track day.id) {
            <article class="routine-card">
              <div class="routine-day-rail"><span>{{ day.nombreDia }}</span></div>
              <div class="routine-card-content">
                <div class="routine-card-heading"><span class="routine-number">{{ day.orden.toString().padStart(2, '0') }}</span><h2>{{ day.enfoque }}</h2></div>
                <div class="routine-exercises">
                  @for (exercise of day.ejercicios; track exercise.id) {
                    <div class="routine-exercise">
                      <span class="exercise-order">{{ exercise.orden.toString().padStart(2, '0') }}</span>
                      <strong>{{ exercise.nombre }}</strong>
                      <span class="exercise-reps">{{ exercise.seriesYRepeticiones }}</span>
                      @if (exercise.notas) { <p class="exercise-note"><span>NOTA</span>{{ exercise.notas }}</p> }
                    </div>
                  } @empty {
                    <p class="routine-empty-day">No hay ejercicios registrados para este día.</p>
                  }
                </div>
              </div>
            </article>
          }
        </div>
      } @else if (errorMessage()) {
        <section class="routine-empty-state">
          <span class="eyebrow">PLAN DE ENTRENAMIENTO</span>
          <h1>{{ errorTitle() }}</h1>
          <p>{{ errorMessage() }}</p>
          <button class="button button-accent" type="button" (click)="loadRoutine()">Volver a intentar</button>
        </section>
      } @else {
        <div class="routine-loading"><span class="eyebrow">OLYMPUS GYM</span><p>Cargando tu rutina…</p></div>
      }
    </section>
  `,
})
export class MiRutinaComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly routine = signal<Rutina | null>(null);
  readonly errorMessage = signal('');
  readonly errorTitle = signal('No encontramos una rutina');

  ngOnInit(): void {
    this.loadRoutine();
  }

  loadRoutine(): void {
    this.errorMessage.set('');
    this.http.get<Rutina>(`${API_BASE_URL}/rutinas/mi-rutina`).subscribe({
      next: (routine) => this.routine.set(routine),
      error: (error: HttpErrorResponse) => {
        this.errorTitle.set(error.status === 404 ? 'Aún no tienes un plan asignado.' : 'No pudimos cargar tu rutina.');
        this.errorMessage.set(error.status === 404
          ? 'Pídele a tu entrenador que configure tu plan de entrenamiento.'
          : 'Revisa tu conexión e intenta cargarla de nuevo.');
      },
    });
  }
}