import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/api.config';

export interface SocioRutina {
  id: string;
  nombre: string;
  apellido: string;
  email: string;
}

interface EjercicioPlantilla {
  orden: number;
  nombre: string;
  seriesYRepeticiones: string;
  notas: string | null;
}

interface DiaPlantilla {
  id: string;
  nombreDia: string;
  orden: number;
  enfoque: string;
  ejercicios: EjercicioPlantilla[];
}

interface PlantillaRutina {
  titulo: string;
  fechaInicio: string | null;
  fechaRevision: string | null;
  dias: DiaPlantilla[];
}

@Component({
  selector: 'app-asignar-rutina-modal',
  imports: [FormsModule, ReactiveFormsModule],
  template: `
    <div class="modal-backdrop routine-assignment-backdrop" (click)="close.emit()" (keydown.escape)="close.emit()">
      <section class="routine-assignment-modal" role="dialog" aria-modal="true" aria-labelledby="routine-modal-title" (click)="$event.stopPropagation()">
        <header class="routine-modal-header">
          <div><span class="eyebrow">GESTIÓN DE ENTRENAMIENTO</span><h2 id="routine-modal-title">Plan de Entrenamiento - {{ selectedMemberName() || fullName(socio()) }}</h2><p>Busca un socio por nombre o correo para asignar su rutina.</p></div>
          <button class="modal-close" type="button" aria-label="Cerrar" (click)="close.emit()">×</button>
        </header>

        <label>Socio / Usuario
          <input name="memberSearch" [(ngModel)]="memberQuery" [ngModelOptions]="{ standalone: true }" [attr.list]="membersListId" autocomplete="off" placeholder="Escribe nombre o correo del socio" (ngModelChange)="onSocioSearchChange($event)" />
          <datalist [id]="membersListId">
            @for (member of members(); track member.id) { <option [value]="fullName(member)" [label]="member.email"></option> }
          </datalist>
          @if (!selectedMemberId() && memberQuery.trim()) { <span class="auth-field-error">Selecciona un socio válido de la lista.</span> }
        </label>

        <label>Plantilla rápida
          <select [value]="selectedTemplate()" (change)="selectTemplate($any($event.target).value)">
            <option value="">Personalizada desde cero</option>
            @for (template of templates(); track template.titulo) { <option [value]="$index">{{ template.titulo }}</option> }
          </select>
        </label>

        <form [formGroup]="form" (ngSubmit)="save()">
          <div class="routine-form-top">
            <label>Título<input formControlName="titulo" maxlength="200" placeholder="Plan de entrenamiento" /></label>
            <label>Fecha de Inicio<input type="date" formControlName="fechaInicio" /></label>
            <label>Fecha de Revisión<input type="date" formControlName="fechaRevision" /></label>
          </div>

          <div class="routine-days-toolbar"><span class="eyebrow">DÍAS DE ENTRENAMIENTO · {{ dias.length }}</span><button class="button button-neon button-small" type="button" (click)="addDay()">+ Agregar Día</button></div>
          <div class="routine-days-list" formArrayName="dias">
            @for (day of dias.controls; track $index; let dayIndex = $index) {
              <section class="routine-edit-day" [formGroupName]="dayIndex">
                <div class="routine-edit-day-heading"><span class="routine-edit-day-number">DÍA {{ (dayIndex + 1).toString().padStart(2, '0') }}</span><button class="routine-remove-button" type="button" (click)="removeDay(dayIndex)">Eliminar día</button></div>
                <div class="routine-day-fields"><label>Día<input formControlName="nombreDia" placeholder="LUNES o DÍA 1" maxlength="30" /></label><label>Enfoque / Grupo muscular<input formControlName="enfoque" placeholder="Pecho + Tríceps" maxlength="150" /></label></div>
                <div class="routine-exercises-list" formArrayName="ejercicios">
                  @for (exercise of exercisesAt(dayIndex).controls; track $index; let exerciseIndex = $index) {
                    <div class="routine-exercise-fields" [formGroupName]="exerciseIndex">
                      <label>Ejercicio<input formControlName="nombre" placeholder="Press banca con barra" maxlength="200" /></label>
                      <label>Series × reps<input formControlName="seriesYRepeticiones" placeholder="4x6-8" maxlength="50" /></label>
                      <label>Notas<input formControlName="notas" placeholder="Calentamiento (opcional)" maxlength="500" /></label>
                      <button class="routine-remove-button exercise-remove" type="button" (click)="removeExercise(dayIndex, exerciseIndex)" aria-label="Eliminar ejercicio">×</button>
                    </div>
                  }
                </div>
                <button class="routine-add-exercise" type="button" (click)="addExercise(dayIndex)">+ Agregar Ejercicio</button>
              </section>
            } @empty { <p class="routine-form-empty">Agrega un día para empezar a armar la rutina.</p> }
          </div>

          @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
          <footer class="routine-modal-footer"><button class="button button-quiet" type="button" (click)="close.emit()">Cancelar</button><button class="button button-neon" type="submit" [disabled]="saving() || form.invalid || !selectedMemberId()">{{ saving() ? 'Guardando…' : 'Guardar y Asignar Rutina' }}</button></footer>
        </form>
      </section>
    </div>
  `,
})
export class AsignarRutinaModalComponent implements OnInit {
  readonly socio = input.required<SocioRutina>();
  readonly close = output<void>();
  readonly assigned = output<void>();
  readonly members = signal<SocioRutina[]>([]);
  readonly selectedMemberId = signal('');
  readonly selectedMemberName = signal('');
  readonly membersListId = `routine-members-${crypto.randomUUID()}`;
  memberQuery = '';
  private readonly http = inject(HttpClient);
  private readonly formBuilder = inject(FormBuilder);
  readonly templates = signal<PlantillaRutina[]>([]);
  readonly selectedTemplate = signal('');
  readonly errorMessage = signal('');
  readonly saving = signal(false);
  readonly form = this.formBuilder.group({
    titulo: ['', [Validators.required, Validators.maxLength(200)]],
    fechaInicio: [new Date().toISOString().slice(0, 10), Validators.required],
    fechaRevision: [''],
    dias: this.formBuilder.array([]),
  });

  get dias(): FormArray {
    return this.form.controls.dias as FormArray;
  }

  ngOnInit(): void {
    const initialMember = this.socio();
    this.selectedMemberId.set(initialMember.id);
    this.selectedMemberName.set(this.fullName(initialMember));
    this.memberQuery = this.fullName(initialMember);
    this.http.get<Array<{ id: string; nombre: string; apellido: string; email: string }>>(`${API_BASE_URL}/admin/usuarios`).subscribe({
      next: users => this.members.set(users.map(user => ({
        id: user.id,
        nombre: user.nombre,
        apellido: user.apellido,
        email: user.email,
      }))),
      error: () => this.errorMessage.set('No se pudo cargar la lista de socios.'),
    });
    this.http.get<PlantillaRutina[]>(`${API_BASE_URL}/admin/rutinas/plantillas`).subscribe({
      next: templates => this.templates.set(templates),
      error: () => this.errorMessage.set('No se pudieron cargar las plantillas. Puedes crear una rutina personalizada.'),
    });
    this.loadRoutine(this.socio().id);
  }

  fullName(member: SocioRutina): string {
    return `${member.nombre} ${member.apellido}`.trim();
  }

  onSocioSearchChange(value: string): void {
    const normalized = value.trim().toLocaleLowerCase();
    const matched = this.members().find(member =>
      this.fullName(member).toLocaleLowerCase() === normalized || member.email.toLocaleLowerCase() === normalized);

    if (!matched) {
      this.selectedMemberId.set('');
      this.selectedMemberName.set('');
      return;
    }

    this.selectedMemberId.set(matched.id);
    this.selectedMemberName.set(this.fullName(matched));
    if (matched.id !== this.loadedMemberId) this.loadRoutine(matched.id);
  }

  exercisesAt(dayIndex: number): FormArray {
    return this.dias.at(dayIndex).get('ejercicios') as FormArray;
  }

  addDay(day?: DiaPlantilla): void {
    this.dias.push(this.formBuilder.group({
      nombreDia: [day?.nombreDia ?? '', Validators.required],
      enfoque: [day?.enfoque ?? '', Validators.required],
      ejercicios: this.formBuilder.array((day?.ejercicios ?? []).map(exercise => this.createExercise(exercise))),
    }));
  }

  removeDay(index: number): void {
    this.dias.removeAt(index);
  }

  addExercise(dayIndex: number): void {
    this.exercisesAt(dayIndex).push(this.createExercise());
  }

  removeExercise(dayIndex: number, exerciseIndex: number): void {
    this.exercisesAt(dayIndex).removeAt(exerciseIndex);
  }

  selectTemplate(value: string): void {
    this.selectedTemplate.set(value);
    this.errorMessage.set('');
    const template = value === '' ? null : this.templates()[Number(value)];
    this.dias.clear();
    if (!template) {
      this.form.patchValue({ titulo: '', fechaInicio: new Date().toISOString().slice(0, 10), fechaRevision: '' });
      return;
    }

    this.form.patchValue({
      titulo: template.titulo,
      fechaInicio: this.dateForInput(template.fechaInicio) ?? new Date().toISOString().slice(0, 10),
      fechaRevision: this.dateForInput(template.fechaRevision) ?? '',
    });
    template.dias.forEach(day => this.addDay(day));
  }

  save(): void {
    const usuarioId = this.selectedMemberId();
    if (!usuarioId) {
      this.errorMessage.set('Selecciona un socio válido por nombre o correo.');
      return;
    }
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      usuarioId,
      titulo: value.titulo?.trim() ?? '',
      fechaInicio: this.dateForRequest(value.fechaInicio)!,
      fechaRevision: this.dateForRequest(value.fechaRevision),
      dias: (value.dias ?? []).map((day: any, dayIndex: number) => ({
        nombreDia: day.nombreDia.trim(),
        orden: dayIndex + 1,
        enfoque: day.enfoque.trim(),
        ejercicios: (day.ejercicios ?? []).map((exercise: any, exerciseIndex: number) => ({
          orden: exerciseIndex + 1,
          nombre: exercise.nombre.trim(),
          seriesYRepeticiones: exercise.seriesYRepeticiones.trim(),
          notas: exercise.notas?.trim() || null,
        })),
      })),
    };

    this.saving.set(true);
    this.errorMessage.set('');
    this.http.post(`${API_BASE_URL}/admin/rutinas/asignar`, request).subscribe({
      next: () => this.assigned.emit(),
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'No se pudo asignar la rutina. Revisa los campos e intenta de nuevo.');
        this.saving.set(false);
      },
      complete: () => this.saving.set(false),
    });
  }

  private createExercise(exercise?: EjercicioPlantilla) {
    return this.formBuilder.group({
      nombre: [exercise?.nombre ?? '', Validators.required],
      seriesYRepeticiones: [exercise?.seriesYRepeticiones ?? '', Validators.required],
      notas: [exercise?.notas ?? ''],
    });
  }

  private loadedMemberId = '';

  private loadRoutine(memberId: string): void {
    this.loadedMemberId = memberId;
    this.errorMessage.set('');
    this.http.get<{ titulo: string; fechaInicio: string | null; fechaRevision: string | null; dias: DiaPlantilla[] }>(
      `${API_BASE_URL}/admin/rutinas/usuario/${memberId}`,
    ).subscribe({
      next: routine => this.populateForm(routine),
      error: (error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.selectedTemplate.set('');
          this.form.patchValue({ titulo: '', fechaInicio: new Date().toISOString().slice(0, 10), fechaRevision: '' });
          this.dias.clear();
          return;
        }
        this.errorMessage.set('No se pudo cargar la rutina actual del socio.');
      },
    });
  }

  private dateForInput(value: string | null): string | null {
    return value && !value.startsWith('0001-01-01') ? value.slice(0, 10) : null;
  }

  private dateForRequest(value: string | null): string | null {
    return value ? `${value}T00:00:00.000Z` : null;
  }

  private populateForm(routine: { titulo: string; fechaInicio: string | null; fechaRevision: string | null; dias: DiaPlantilla[] }): void {
    this.form.patchValue({
      titulo: routine.titulo,
      fechaInicio: this.dateForInput(routine.fechaInicio) ?? new Date().toISOString().slice(0, 10),
      fechaRevision: this.dateForInput(routine.fechaRevision) ?? '',
    });
    this.dias.clear();
    routine.dias.forEach(day => this.addDay(day));
  }
}