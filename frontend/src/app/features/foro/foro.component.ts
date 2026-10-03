import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/api.config';
import { AuthService } from '../../core/auth.service';

interface Publicacion {
  id: number;
  titulo: string;
  contenido: string;
  imagenUrl: string | null;
  fechaPublicacion: string;
  autorNombre: string;
}

@Component({
  selector: 'app-foro',
  imports: [FormsModule, DatePipe],
  template: `
    <section class="content-page">
      <div class="page-heading heading-split">
        <div><span class="eyebrow">OLYMPUS · COMUNIDAD</span><h1>Cartelera</h1><p class="lede">Novedades, eventos y avisos del gimnasio.</p></div>
        @if (auth.isAdmin()) { <button class="button button-accent" type="button" (click)="toggleForm()">{{ editingId ? 'Cancelar edición' : '+ Nueva publicación' }}</button> }
      </div>
      @if (showForm) {
        <form class="editor-panel" (ngSubmit)="save()">
          <h2>{{ editingId ? 'Editar publicación' : 'Nueva publicación' }}</h2>
          <label>Título<input name="titulo" [(ngModel)]="draft.titulo" required maxlength="200" /></label>
          <label>Contenido<textarea name="contenido" [(ngModel)]="draft.contenido" required rows="5" maxlength="10000"></textarea></label>
          <label>URL de imagen<input name="imagenUrl" type="url" [(ngModel)]="draft.imagenUrl" /></label>
          <div class="form-actions"><button class="button button-dark" type="submit">Publicar</button><button class="button button-quiet" type="button" (click)="toggleForm()">Cancelar</button></div>
        </form>
      }
      @if (errorMessage()) { <p class="form-error" role="alert">{{ errorMessage() }}</p> }
      @if (!posts().length) { <div class="empty-state"><span class="empty-mark">O</span><h2>Todo en calma.</h2><p>Las novedades del gimnasio aparecerán aquí.</p></div> }
      <div class="news-grid">
        @for (post of posts(); track post.id) {
          <article class="news-item">
            @if (post.imagenUrl) { <img class="news-image" [src]="post.imagenUrl" [alt]="post.titulo" /> }
            <div class="news-body"><div class="news-meta"><span>{{ post.autorNombre }}</span><time>{{ post.fechaPublicacion | date:'d MMM y' }}</time></div>
              <h2>{{ post.titulo }}</h2><p>{{ post.contenido }}</p>
              @if (auth.isAdmin()) { <div class="item-actions"><button class="text-button" type="button" (click)="edit(post)">Editar</button><button class="text-button danger-text" type="button" (click)="remove(post.id)">Eliminar</button></div> }
            </div>
          </article>
        }
      </div>
    </section>
  `,
})
export class ForoComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  readonly posts = signal<Publicacion[]>([]);
  showForm = false;
  editingId: number | null = null;
  readonly errorMessage = signal('');
  draft = { titulo: '', contenido: '', imagenUrl: '' };

  ngOnInit(): void { this.load(); }

  load(): void {
    this.http.get<Publicacion[]>(`${API_BASE_URL}/foro`).subscribe({
      next: (posts) => this.posts.set(posts),
      error: () => this.errorMessage.set('No se pudieron cargar las publicaciones.'),
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    this.editingId = null;
    this.draft = { titulo: '', contenido: '', imagenUrl: '' };
  }

  edit(post: Publicacion): void {
    this.editingId = post.id;
    this.draft = { titulo: post.titulo, contenido: post.contenido, imagenUrl: post.imagenUrl ?? '' };
    this.showForm = true;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  save(): void {
    const payload = { ...this.draft, imagenUrl: this.draft.imagenUrl || null };
    const request = this.editingId
      ? this.http.put(`${API_BASE_URL}/foro/${this.editingId}`, payload)
      : this.http.post(`${API_BASE_URL}/foro`, payload);
    request.subscribe({
      next: () => { this.toggleForm(); this.load(); },
      error: (error: HttpErrorResponse) => this.errorMessage.set(error.error?.message ?? 'No se pudo guardar la publicación.'),
    });
  }

  remove(id: number): void {
    if (!window.confirm('¿Eliminar esta publicación?')) return;
    this.http.delete(`${API_BASE_URL}/foro/${id}`).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('No se pudo eliminar la publicación.'),
    });
  }
}