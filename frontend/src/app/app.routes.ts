import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { adminGuard } from './core/admin.guard';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'foro' },
	{ path: 'login', loadComponent: () => import('./features/auth/login.component').then((module) => module.LoginComponent) },
	{ path: 'registro', loadComponent: () => import('./features/auth/register.component').then((module) => module.RegisterComponent) },
	{ path: 'foro', canActivate: [authGuard], loadComponent: () => import('./features/foro/foro.component').then((module) => module.ForoComponent) },
	{ path: 'admin', canActivate: [adminGuard], loadComponent: () => import('./features/admin-dashboard/admin-dashboard.component').then((module) => module.AdminDashboardComponent) },
	{ path: 'perfil', canActivate: [authGuard], loadComponent: () => import('./features/perfil/perfil.component').then((module) => module.PerfilComponent) },
	{ path: 'rutina', canActivate: [authGuard], loadComponent: () => import('./features/rutina/mi-rutina.component').then((module) => module.MiRutinaComponent) },
	{ path: 'membresias', canActivate: [authGuard], loadComponent: () => import('./features/membresias/membresias.component').then((module) => module.MembresiasComponent) },
	{ path: '**', redirectTo: 'foro' },
];
