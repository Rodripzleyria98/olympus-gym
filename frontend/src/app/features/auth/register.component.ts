import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { OlympusLogoComponent } from '../../shared/olympus-logo.component';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink, OlympusLogoComponent],
  template: `
    <section class="auth-layout">
      <div class="auth-aside">
        <span class="eyebrow">TU ESPACIO. TU PROGRESO.</span>
        <app-olympus-logo />
        <p class="auth-caption">Entrena con<br />propósito.</p>
      </div>
      <form #registerForm="ngForm" class="auth-form" (ngSubmit)="submit(registerForm)">
        <span class="eyebrow">NUEVA CUENTA</span>
        <h1>Hazte<br />socio.</h1>
        <div class="form-row">
          <label>Nombre<input name="nombre" [(ngModel)]="nombre" autocomplete="given-name" maxlength="100" required /></label>
          <label>Apellido<input name="apellido" [(ngModel)]="apellido" autocomplete="family-name" maxlength="100" required /></label>
        </div>
        <label>Correo electrónico
          <input #emailModel="ngModel" name="email" type="email" [(ngModel)]="email" autocomplete="email" maxlength="254" email required />
          @if (emailModel.invalid && (emailModel.dirty || emailModel.touched)) { <span class="auth-field-error">Escribe un correo electrónico válido.</span> }
        </label>
        <label>Contraseña
          <input #passwordModel="ngModel" name="password" type="password" [(ngModel)]="password" [pattern]="passwordPattern" maxlength="100" autocomplete="new-password" aria-describedby="password-rules" required />
        </label>
        <div id="password-rules" class="password-guidance" aria-live="polite">
          <span>La contraseña debe incluir:</span>
          <ul>
            <li [class.rule-met]="password.length >= 8">Al menos 8 caracteres</li>
            <li [class.rule-met]="hasUppercase()">Una letra mayúscula</li>
            <li [class.rule-met]="hasNumber()">Un número</li>
            <li [class.rule-met]="hasSpecialCharacter()">Un carácter especial (por ejemplo, ! @ #)</li>
          </ul>
          @if (passwordModel.invalid && (passwordModel.dirty || passwordModel.touched)) { <span class="auth-field-error">Completa todos los requisitos para continuar.</span> }
        </div>
        @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
        <button class="button button-accent button-wide" type="submit" [disabled]="loading() || registerForm.invalid">
          {{ loading() ? 'Creando cuenta…' : 'Crear cuenta' }}
        </button>
        <p class="auth-footnote">¿Ya tienes cuenta? <a routerLink="/login">Inicia sesión</a></p>
      </form>
    </section>
  `,
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly passwordPattern = '(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9\\s]).{8,100}';
  nombre = '';
  apellido = '';
  email = '';
  password = '';
  readonly errorMessage = signal('');
  readonly loading = signal(false);

  hasUppercase(): boolean {
    return /[A-Z]/.test(this.password);
  }

  hasNumber(): boolean {
    return /\d/.test(this.password);
  }

  hasSpecialCharacter(): boolean {
    return /[^A-Za-z0-9\s]/.test(this.password);
  }

  submit(form: NgForm): void {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.auth.register({ nombre: this.nombre, apellido: this.apellido, email: this.email, password: this.password })
      .pipe(switchMap(() => this.auth.login(this.email, this.password)))
      .subscribe({
        next: () => void this.router.navigateByUrl('/foro'),
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(error.error?.message ?? 'No fue posible crear la cuenta. Revisa tus datos.');
          this.loading.set(false);
        },
        complete: () => this.loading.set(false),
      });
  }
}