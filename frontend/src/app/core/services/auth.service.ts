import { Injectable, inject, signal, computed } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oidc = inject(OidcSecurityService);

  // Signals for reactive auth state across the app
  readonly isAuthenticated = toSignal(
    this.oidc.isAuthenticated$.pipe(map(({ isAuthenticated }) => isAuthenticated)),
    { initialValue: false }
  );

  readonly userData = toSignal(
    this.oidc.userData$.pipe(map(({ userData }) => userData)),
    { initialValue: null }
  );

  readonly userName = computed(() => {
    const data = this.userData();
    return data?.name ?? data?.preferred_username ?? 'User';
  });

  readonly userEmail = computed(() => {
    const data = this.userData();
    return data?.preferred_username ?? data?.email ?? '';
  });

  login(): void {
    this.oidc.authorize();
  }

  logout(): void {
    this.oidc.logoff().subscribe();
  }

  checkAuth() {
    return this.oidc.checkAuth();
  }
}
