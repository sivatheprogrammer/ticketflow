import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { switchMap, take } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * Attaches the Bearer access token to all requests going to the API.
 *
 * Why a custom interceptor when angular-auth-oidc-client has secureRoutes?
 * secureRoutes handles basic cases, but this interceptor gives us explicit
 * control — we can add logging, handle token refresh edge cases, and
 * the pattern is identical whether we're using Entra or Okta tokens.
 *
 * In Phase 2 this replaces the Phase 1 pattern of no auth at all.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const oidcService = inject(OidcSecurityService);

  // Only attach token to our API calls — not to CDN or other third-party requests
  if (!req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  return oidcService.getAccessToken().pipe(
    take(1),
    switchMap(token => {
      if (token) {
        const authReq = req.clone({
          setHeaders: { Authorization: `Bearer ${token}` }
        });
        return next(authReq);
      }
      return next(req);
    })
  );
};
