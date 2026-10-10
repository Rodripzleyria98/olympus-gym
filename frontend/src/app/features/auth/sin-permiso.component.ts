import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-sin-permiso',
  imports: [RouterLink],
  template: `
    <section class="content-page permission-page">
      <span class="eyebrow">ACCESO RESTRINGIDO</span>
      <h1>Sin permiso</h1>
      <p class="lede">Tu cuenta no tiene acceso a esta sección.</p>
      <a class="button button-dark" routerLink="/foro">Volver al inicio</a>
    </section>
  `,
})
export class SinPermisoComponent {}