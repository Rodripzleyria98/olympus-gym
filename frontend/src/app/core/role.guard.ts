import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) return router.createUrlTree(['/login']);

  const roles = route.data['roles'] as string[] | undefined;
  return !roles?.length || roles.some((role) => auth.hasRole(role))
    ? true
    : router.createUrlTree(['/sin-permiso']);
};