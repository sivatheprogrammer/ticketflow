import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, take } from 'rxjs';

/**
 * Route guard — redirects unauthenticated users to login.
 * Used on /my-bookings and /bookings/:id routes.
 * Phase 2: replaces the open access from Phase 1.
 */
export const authGuard: CanActivateFn = () => {
  const oidcService = inject(OidcSecurityService);
  const router = inject(Router);

  return oidcService.isAuthenticated$.pipe(
    take(1),
    map(({ isAuthenticated }) => {
      if (isAuthenticated) return true;
      // Redirect to home — the login button is in the header
      router.navigate(['/events']);
      return false;
    })
  );
};
