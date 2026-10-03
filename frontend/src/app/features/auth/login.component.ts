import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { OlympusLogoComponent } from '../../shared/olympus-logo.component';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink, OlympusLogoComponent],
  template: `
    <section class="auth-layout">
      <div class="auth-aside">
        <span class="eyebrow">DISCIPLINA · FUERZA · COMUNIDAD</span>
        <app-olympus-logo />
        <p class="auth-caption">Tu próximo nivel<br />empieza aquí.</p>
      </div>
      <form class="auth-form" (ngSubmit)="submit()">
        <span class="eyebrow">PORTAL DE SOCIOS</span>
        <h1>Bienvenido<br />de vuelta.</h1>
        <label>Correo electrónico<input name="email" type="email" [(ngModel)]="email" autocomplete="email" required /></label>
        <label>Contraseña<input name="password" type="password" [(ngModel)]="password" autocomplete="current-password" required /></label>
        @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
        <button class="button button-accent button-wide" type="submit" [disabled]="loading()">
          {{ loading() ? 'Validando…' : 'Entrar a mi cuenta' }}
        </button>
        <p class="auth-footnote">¿Aún no eres socio? <a routerLink="/registro">Crea tu cuenta</a></p>
      </form>
    </section>
  `,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  email = '';
  password = '';
  readonly errorMessage = signal('');
  readonly loading = signal(false);

  submit(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.auth.login(this.email, this.password).subscribe({
      next: (response) => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        void this.router.navigateByUrl(returnUrl || (response.usuario.rol === 'Admin' ? '/admin' : '/foro'));
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'No fue posible conectar con Olympus. Intenta de nuevo.');
        this.loading.set(false);
      },
      complete: () => this.loading.set(false),
    });
  }
}