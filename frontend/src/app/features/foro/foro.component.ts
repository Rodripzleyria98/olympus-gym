import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { API_BASE_URL } from '../../core/api.config';
import { AuthService } from '../../core/auth.service';
import { normalizePagedResponse, PagedResponse } from '../../core/paged-response';

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
      <div class="toolbar">
        <label class="search-field"><span aria-hidden="true">⌕</span><input aria-label="Buscar publicaciones" placeholder="Buscar publicaciones" [(ngModel)]="search" (ngModelChange)="onSearchChange($event)" /></label>
        <span class="result-count">{{ totalCount() }} publicaciones</span>
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
      @if (totalPages() > 1) {
        <nav class="pagination" aria-label="Paginación de publicaciones">
          <button type="button" (click)="goToPage(page() - 1)" [disabled]="page() <= 1">Anterior</button>
          @for (pageNumber of pageNumbers(); track pageNumber) {
            <button type="button" [class.current]="pageNumber === page()" [attr.aria-current]="pageNumber === page() ? 'page' : null" (click)="goToPage(pageNumber)">{{ pageNumber }}</button>
          }
          <button type="button" (click)="goToPage(page() + 1)" [disabled]="page() >= totalPages()">Siguiente</button>
        </nav>
      }
    </section>
  `,
})
export class ForoComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchChanges = new Subject<string>();
  readonly posts = signal<Publicacion[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly totalCount = signal(0);
  readonly pageSize = 10;
  search = '';
  showForm = false;
  editingId: number | null = null;
  readonly errorMessage = signal('');
  draft = { titulo: '', contenido: '', imagenUrl: '' };

  ngOnInit(): void {
    this.searchChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => {
      this.page.set(1);
      this.load();
    });
    this.load();
  }

  load(): void {
    let params = new HttpParams().set('page', this.page()).set('pageSize', this.pageSize);
    const search = this.search.trim();
    if (search) params = params.set('search', search);
    this.http.get<PagedResponse<Publicacion> | Publicacion[]>(`${API_BASE_URL}/foro`, { params }).subscribe({
      next: (response) => {
        const filteredResponse = Array.isArray(response) && search
          ? response.filter((post) => `${post.titulo} ${post.contenido} ${post.autorNombre}`.toLocaleLowerCase().includes(search.toLocaleLowerCase()))
          : response;
        const result = normalizePagedResponse(filteredResponse, this.page(), this.pageSize);
        this.posts.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: () => this.errorMessage.set('No se pudieron cargar las publicaciones.'),
    });
  }

  onSearchChange(value: string): void {
    this.searchChanges.next(value.trim());
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) return;
    this.page.set(page);
    this.load();
  }

  pageNumbers(): number[] {
    const first = Math.max(1, this.page() - 2);
    const last = Math.min(this.totalPages(), this.page() + 2);
    return Array.from({ length: last - first + 1 }, (_, index) => first + index);
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