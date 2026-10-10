import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { roleGuard } from './core/role.guard';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'foro' },
	{ path: 'login', loadComponent: () => import('./features/auth/login.component').then((module) => module.LoginComponent) },
	{ path: 'registro', loadComponent: () => import('./features/auth/register.component').then((module) => module.RegisterComponent) },
	{ path: 'foro', canActivate: [authGuard], loadComponent: () => import('./features/foro/foro.component').then((module) => module.ForoComponent) },
	{ path: 'admin', canActivate: [roleGuard], data: { roles: ['Admin'] }, loadComponent: () => import('./features/admin-dashboard/admin-dashboard.component').then((module) => module.AdminDashboardComponent) },
	{ path: 'sin-permiso', loadComponent: () => import('./features/auth/sin-permiso.component').then((module) => module.SinPermisoComponent) },
	{ path: 'perfil', canActivate: [authGuard], loadComponent: () => import('./features/perfil/perfil.component').then((module) => module.PerfilComponent) },
	{ path: 'rutina', canActivate: [authGuard], loadComponent: () => import('./features/rutina/mi-rutina.component').then((module) => module.MiRutinaComponent) },
	{ path: 'membresias', canActivate: [authGuard], loadComponent: () => import('./features/membresias/membresias.component').then((module) => module.MembresiasComponent) },
	{ path: '**', redirectTo: 'foro' },
];
