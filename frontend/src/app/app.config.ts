import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideAuth, LogLevel } from 'angular-auth-oidc-client';

import { routes } from './app.routes';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideAnimationsAsync(),
    provideAuth({
  config: {
    authority: environment.auth.authority,
    redirectUrl: environment.auth.redirectUrl,
    postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
    clientId: environment.auth.clientId,
    scope: environment.auth.scope,
    responseType: environment.auth.responseType,
    silentRenew: false,
    useRefreshToken: false,
    maxIdTokenIatOffsetAllowedInSeconds: 600,
    logLevel: LogLevel.Warn,
    secureRoutes: [environment.apiBaseUrl],
  }
})
  ]
};