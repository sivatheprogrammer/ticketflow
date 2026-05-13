import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

/**
 * Functional HTTP interceptor (Angular 18+ standard).
 * Centralized error handling. In Phase 2, this is where we'll attach
 * the OAuth Bearer token and handle 401 / token-refresh logic.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snack = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const message = err.error?.detail || err.message || 'An unexpected error occurred';
      snack.open(message, 'Dismiss', { duration: 5000, panelClass: 'error-snack' });
      return throwError(() => err);
    })
  );
};
