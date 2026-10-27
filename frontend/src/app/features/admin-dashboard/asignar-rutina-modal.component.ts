import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { normalizePagedResponse, PagedResponse } from '../../core/paged-response';
import { parseRoutineText } from './routine-text-parser';

export interface SocioRutina {
  id: string;
  nombre: string;
  apellido: string;
  email: string;
  planMembresia: string | null;
  clasesIncluidas: number | null;
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

interface RoutineDayInput {
  nombreDia: string;
  enfoque: string;
  ejercicios: Array<Pick<EjercicioPlantilla, 'nombre' | 'seriesYRepeticiones' | 'notas'>>;
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
          @if (planNotice()) { <p class="muted" role="status">{{ planNotice() }}</p> }
          <div class="routine-days-list" formArrayName="dias">
            @for (day of dias.controls; track $index; let dayIndex = $index) {
              <section class="routine-edit-day" [formGroupName]="dayIndex">
                <div class="routine-edit-day-heading">
                  <span class="routine-edit-day-number">DÍA {{ (dayIndex + 1).toString().padStart(2, '0') }}</span>
                  <div class="day-actions-header">
                    <button class="btn-paste-day" type="button" (click)="openDayImport(dayIndex)">Pegar rutina de este día</button>
                    <button class="routine-remove-button" type="button" (click)="removeDay(dayIndex)">Eliminar día</button>
                  </div>
                </div>
                <div class="routine-day-fields"><label>Día<input formControlName="nombreDia" placeholder="LUNES o DÍA 1" maxlength="30" /></label><label>Enfoque / Grupo muscular<input formControlName="enfoque" placeholder="Pecho + Tríceps" maxlength="150" /></label></div>
                <div class="routine-exercises-list" formArrayName="ejercicios">
                  @for (exercise of exercisesAt(dayIndex).controls; track $index; let exerciseIndex = $index) {
                    <div class="routine-exercise-fields" [formGroupName]="exerciseIndex">
                      <label>Ejercicio<input formControlName="nombre" placeholder="Press banca con barra" maxlength="200" /></label>
                      <label>Series × reps<input formControlName="seriesYRepeticiones" placeholder="4x6-8" maxlength="50" /></label>
                      <label>Notas<input formControlName="notas" placeholder="Calentamiento (opcional)" maxlength="500" /></label>
                      <div><button class="text-button" type="button" [disabled]="exerciseIndex === 0" (click)="moveExercise(dayIndex, exerciseIndex, -1)">↑</button><button class="text-button" type="button" [disabled]="exerciseIndex === exercisesAt(dayIndex).length - 1" (click)="moveExercise(dayIndex, exerciseIndex, 1)">↓</button><button class="routine-remove-button exercise-remove" type="button" (click)="removeExercise(dayIndex, exerciseIndex)" aria-label="Eliminar ejercicio">×</button></div>
                    </div>
                  }
                </div>
                <button class="routine-add-exercise" type="button" (click)="addExercise(dayIndex)">+ Agregar Ejercicio</button>
              </section>
            } @empty { <p class="routine-form-empty">Agrega un día para empezar a armar la rutina.</p> }
          </div>

          @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
          @if (showValidationErrors() && getFormValidationErrors().length) {
            <ul class="form-error" role="alert">
              @for (validationError of getFormValidationErrors(); track validationError) { <li>{{ validationError }}</li> }
            </ul>
          }
          <footer class="routine-modal-footer"><button class="button button-quiet" type="button" (click)="close.emit()">Cancelar</button><button class="button button-neon" type="submit" [disabled]="saving() || !selectedMemberId()">{{ saving() ? 'Guardando…' : 'Guardar y Asignar Rutina' }}</button></footer>
        </form>
        @if (!importModalOpen() && importMessage()) { <p [class.form-error]="importHasErrors()" role="status">{{ importMessage() }}</p> }

        @if (importModalOpen()) {
          <div class="modal-backdrop routine-day-import-backdrop" (click)="closeDayImport()" (keydown.escape)="closeDayImport(); $event.stopPropagation()">
            <section class="routine-assignment-modal" role="dialog" aria-modal="true" aria-labelledby="routine-import-title" (click)="$event.stopPropagation()">
              <header class="routine-modal-header">
                <div><span class="eyebrow">CARGA RÁPIDA · {{ dayImportName() }}</span><h2 id="routine-import-title">Pegar rutina de este día</h2><p>Pega los ejercicios de este día. Se agregarán a sus filas y podrás editarlos antes de guardar.</p></div>
                <button class="modal-close" type="button" aria-label="Cerrar importador" (click)="closeDayImport()">×</button>
              </header>
              <label>Ejercicios<textarea name="routineText" [(ngModel)]="routineText" [ngModelOptions]="{ standalone: true }" rows="14" placeholder="1. Press de banca con barra — 4 x 6-8&#10;2. Press inclinado con mancuernas — 3 x 8-10&#10;3. Aperturas en polea — 3 x 12-15"></textarea></label>
              <p class="muted">Puedes incluir un encabezado de día, pero pega ejercicios de un solo día. Las líneas sin series/repeticiones se conservan como nombre del ejercicio para que puedas completarlas.</p>
              @if (importMessage()) { <p [class.form-error]="importHasErrors()" role="status">{{ importMessage() }}</p> }
              <footer class="routine-modal-footer">
                <button class="button button-quiet" type="button" (click)="closeDayImport()">Cancelar</button>
                <button class="button button-neon" type="button" (click)="processDayText()">Procesar / Cargar en este día</button>
              </footer>
            </section>
          </div>
        }
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
  private readonly destroyRef = inject(DestroyRef);
  private readonly memberSearchTerms = new Subject<string>();
  private readonly formBuilder = inject(FormBuilder);
  readonly templates = signal<PlantillaRutina[]>([]);
  readonly selectedTemplate = signal('');
  readonly errorMessage = signal('');
  readonly importMessage = signal('');
  readonly importHasErrors = signal(false);
  readonly importModalOpen = signal(false);
  readonly saving = signal(false);
  readonly showValidationErrors = signal(false);
  readonly planNotice = signal('');
  pasteTargetDay = 0;
  routineText = '';
  readonly form = this.formBuilder.group({
    titulo: ['', [Validators.required, Validators.maxLength(200)]],
    fechaInicio: [new Date().toISOString().slice(0, 10), Validators.required],
    fechaRevision: [''],
    dias: this.formBuilder.array([], Validators.minLength(1)),
  });

  get dias(): FormArray {
    return this.form.controls.dias as FormArray;
  }

  ngOnInit(): void {
    const initialMember = this.socio();
    this.selectedMemberId.set(initialMember.id);
    this.selectedMemberName.set(this.fullName(initialMember));
    this.memberQuery = this.fullName(initialMember);
    this.members.set([initialMember]);
    this.memberSearchTerms.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((term) => {
        let params = new HttpParams().set('page', 1).set('pageSize', 50);
        if (term) params = params.set('buscar', term);
        return this.http.get<PagedResponse<SocioRutina> | SocioRutina[]>(`${API_BASE_URL}/admin/usuarios`, { params });
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe({
      next: response => {
        const { items } = normalizePagedResponse(response, 1, 50);
        this.members.set(items);
        const query = this.memberQuery.trim().toLocaleLowerCase();
        const match = items.find(member =>
          this.fullName(member).toLocaleLowerCase() === query || member.email.toLocaleLowerCase() === query);
        if (match) {
          if (match.id !== this.selectedMemberId()) this.resetRoutineForm();
          this.selectedMemberId.set(match.id);
          this.selectedMemberName.set(this.fullName(match));
          this.applyMembershipSuggestion(match);
        }
      },
      error: () => this.errorMessage.set('No se pudo cargar la lista de socios.'),
    });
    this.http.get<PlantillaRutina[]>(`${API_BASE_URL}/admin/rutinas/plantillas`).subscribe({
      next: templates => this.templates.set(templates),
      error: () => this.errorMessage.set('No se pudieron cargar las plantillas. Puedes crear una rutina personalizada.'),
    });
    this.resetRoutineForm();
    this.applyMembershipSuggestion(initialMember);
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
    } else {
      if (matched.id !== this.selectedMemberId()) this.resetRoutineForm();
      this.selectedMemberId.set(matched.id);
      this.selectedMemberName.set(this.fullName(matched));
      this.applyMembershipSuggestion(matched);
    }
    this.memberSearchTerms.next(value.trim());
  }

  exercisesAt(dayIndex: number): FormArray {
    return this.dias.at(dayIndex).get('ejercicios') as FormArray;
  }

  addDay(day?: RoutineDayInput): void {
    this.dias.push(this.formBuilder.group({
      nombreDia: [day?.nombreDia ?? '', [Validators.required, Validators.maxLength(30)]],
      enfoque: [day?.enfoque ?? '', [Validators.required, Validators.maxLength(150)]],
      ejercicios: this.formBuilder.array((day?.ejercicios ?? []).map(exercise => this.createExercise(exercise))),
    }));
  }

  removeDay(index: number): void {
    this.dias.removeAt(index);
    if (this.pasteTargetDay >= this.dias.length) this.pasteTargetDay = Math.max(0, this.dias.length - 1);
  }

  addExercise(dayIndex: number): void {
    this.exercisesAt(dayIndex).push(this.createExercise());
  }

  removeExercise(dayIndex: number, exerciseIndex: number): void {
    this.exercisesAt(dayIndex).removeAt(exerciseIndex);
  }

  moveExercise(dayIndex: number, exerciseIndex: number, offset: -1 | 1): void {
    const exercises = this.exercisesAt(dayIndex);
    const targetIndex = exerciseIndex + offset;
    if (targetIndex < 0 || targetIndex >= exercises.length) return;
    const exercise = exercises.at(exerciseIndex);
    exercises.removeAt(exerciseIndex);
    exercises.insert(targetIndex, exercise);
  }

  openDayImport(dayIndex: number): void {
    this.pasteTargetDay = dayIndex;
    this.routineText = '';
    this.importMessage.set('');
    this.importHasErrors.set(false);
    this.importModalOpen.set(true);
  }

  closeDayImport(): void {
    this.importModalOpen.set(false);
  }

  dayImportName(): string {
    const name = this.dias.at(this.pasteTargetDay)?.get('nombreDia')?.value as string | undefined;
    return name?.trim() || `Día ${this.pasteTargetDay + 1}`;
  }

  processDayText(): void {
    const parsedDays = parseRoutineText(this.routineText);
    if (!parsedDays.length) {
      this.importMessage.set('Pega al menos una línea con un ejercicio.');
      this.importHasErrors.set(true);
      return;
    }
    if (parsedDays.length !== 1) {
      this.importMessage.set('Este panel carga un solo día. Quita los otros encabezados o importa cada día por separado.');
      this.importHasErrors.set(true);
      return;
    }

    if (this.pasteTargetDay < 0 || this.pasteTargetDay >= this.dias.length) {
      this.importMessage.set('El día seleccionado ya no existe. Cierra el importador y vuelve a abrirlo desde el día correspondiente.');
      this.importHasErrors.set(true);
      return;
    }

    const exercises = parsedDays[0].ejercicios;
    exercises.forEach(exercise => this.exercisesAt(this.pasteTargetDay).push(this.createExercise(exercise)));
    this.selectedTemplate.set('');
    const unparsedCount = exercises.filter(exercise => !exercise.seriesYRepeticiones).length;
    this.importHasErrors.set(unparsedCount > 0);
    this.importMessage.set(`Se agregaron ${exercises.length} ejercicio(s) en ${this.dayImportName()}.` +
      (unparsedCount ? ` Completa las series y repeticiones de ${unparsedCount} ejercicio(s).` : ''));
    this.closeDayImport();
  }

  selectTemplate(value: string): void {
    this.selectedTemplate.set(value);
    this.errorMessage.set('');
    this.importMessage.set('');
    const template = value === '' ? null : this.templates()[Number(value)];
    this.dias.clear();
    if (!template) {
      this.form.patchValue({ titulo: '', fechaInicio: new Date().toISOString().slice(0, 10), fechaRevision: '' });
      this.addDay();
      this.addExercise(0);
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
      this.showValidationErrors.set(true);
      this.errorMessage.set('Completa los campos obligatorios antes de guardar.');
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
    this.showValidationErrors.set(false);
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

  private createExercise(exercise?: Pick<EjercicioPlantilla, 'nombre' | 'seriesYRepeticiones' | 'notas'>) {
    return this.formBuilder.group({
      nombre: [exercise?.nombre ?? '', [Validators.required, Validators.maxLength(200)]],
      seriesYRepeticiones: [exercise?.seriesYRepeticiones ?? '', [Validators.required, Validators.maxLength(50)]],
      notas: [exercise?.notas ?? '', Validators.maxLength(500)],
    });
  }

  private resetRoutineForm(): void {
    this.selectedTemplate.set('');
    this.routineText = '';
    this.importMessage.set('');
    this.importHasErrors.set(false);
    this.importModalOpen.set(false);
    this.showValidationErrors.set(false);
    this.planNotice.set('');
    this.pasteTargetDay = 0;
    this.form.patchValue({ titulo: '', fechaInicio: new Date().toISOString().slice(0, 10), fechaRevision: '' });
    this.dias.clear();
    this.addDay();
    this.addExercise(0);
  }

  getFormValidationErrors(): string[] {
    const errors: string[] = [];
    const collect = (control: AbstractControl, path: string): void => {
      if (control.errors) {
        if (control.errors['required']) errors.push(`${path}: este campo es obligatorio.`);
        if (control.errors['maxlength']) errors.push(`${path}: supera el máximo permitido.`);
        if (control.errors['minlength']) errors.push(`${path}: agrega al menos un día.`);
      }
      if (control instanceof FormArray || control instanceof FormGroup) {
        Object.keys(control.controls).forEach(key => {
          const childPath = control instanceof FormArray
            ? `Día ${Number(key) + 1}`
            : `${path} > ${key}`;
          const child = control instanceof FormArray
            ? control.at(Number(key))
            : control.controls[key];
          collect(child, childPath);
        });
      }
    };
    collect(this.form, 'Formulario');
    return [...new Set(errors)];
  }

  private applyMembershipSuggestion(member: SocioRutina): void {
    const weeklyAccesses = member.clasesIncluidas === null
      ? null
      : Math.max(1, Math.min(7, Math.round(member.clasesIncluidas / 4)));
    if (weeklyAccesses === null) {
      this.planNotice.set(member.planMembresia
        ? `${member.planMembresia}: puedes asignar entre 1 y 7 días según la necesidad del socio.`
        : 'Sin membresía activa informada: puedes asignar entre 1 y 7 días.');
      return;
    }

    this.planNotice.set(`${member.planMembresia ?? 'Membresía activa'}: se sugieren ${weeklyAccesses} días por semana. Puedes ajustar la cantidad sin bloquear el guardado.`);
    if (this.dias.length === 1 && this.isEmptyDay(this.dias.at(0))) {
      while (this.dias.length < weeklyAccesses) this.addDay();
      while (this.dias.length > weeklyAccesses) this.dias.removeAt(this.dias.length - 1);
      for (let index = 0; index < this.dias.length; index++) {
        if (!this.exercisesAt(index).length) this.addExercise(index);
      }
    }
  }

  private isEmptyDay(day: AbstractControl): boolean {
    const exercises = day.get('ejercicios') as FormArray;
    return !day.get('nombreDia')?.value && !day.get('enfoque')?.value &&
      exercises.controls.every(exercise =>
        !exercise.get('nombre')?.value && !exercise.get('seriesYRepeticiones')?.value &&
        !exercise.get('notas')?.value);
  }

  private dateForInput(value: string | null): string | null {
    return value && !value.startsWith('0001-01-01') ? value.slice(0, 10) : null;
  }

  private dateForRequest(value: string | null): string | null {
    return value ? `${value}T00:00:00.000Z` : null;
  }

}