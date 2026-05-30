import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary" style="gap:8px;padding:0 16px">
      <a routerLink="/events"
         style="color:white;text-decoration:none;font-size:1.2rem;font-weight:500">
        🎟 TicketFlow
      </a>
      <span style="flex:1"></span>
      <a mat-button routerLink="/events" style="color:white">Events</a>
      @if (auth.isAuthenticated()) {
        <span style="color:white;margin:0 12px;font-size:0.9rem">
          {{ auth.userName() }}
        </span>
        <button mat-button style="color:white" (click)="auth.logout()">
          Sign out
        </button>
      } @else {
        <button mat-button style="color:white" (click)="auth.login()">
          Sign in
        </button>
      }
    </mat-toolbar>
  `
})
export class HeaderComponent {
  readonly auth = inject(AuthService);
}