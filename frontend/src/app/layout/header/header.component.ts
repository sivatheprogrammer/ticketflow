import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary">
      <mat-icon>confirmation_number</mat-icon>
      <a routerLink="/" class="brand">TicketFlow</a>
      <span class="spacer"></span>
      <a mat-button routerLink="/events" routerLinkActive="active-link">
        Events
      </a>
      <a mat-button routerLink="/my-bookings" routerLinkActive="active-link">
        My Bookings
      </a>
    </mat-toolbar>
  `,
  styles: [`
    mat-toolbar { gap: 8px; }
    .brand {
      font-size: 1.2rem;
      font-weight: 500;
      text-decoration: none;
      color: white;
      margin-left: 8px;
    }
    .spacer { flex: 1; }
    .active-link { background: rgba(255,255,255,0.15); border-radius: 4px; }
    a[mat-button] { color: white; }
  `]
})
export class HeaderComponent {}
