import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { BookingsService } from '../../core/services/bookings.service';
import { AuthService } from '../../core/services/auth.service';
import { Booking, BookingStatus } from '../../core/models';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatChipsModule
  ],
  template: `
    <h1 style="margin-bottom:16px">
      My Bookings
      @if (authService.userName()) {
        <span style="font-size:1rem;font-weight:400;color:#666;margin-left:8px">
          — {{ authService.userName() }}
        </span>
      }
    </h1>

    @if (loading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (bookings().length === 0) {
      <mat-card>
        <mat-card-content style="padding:24px">
          <p>You have no bookings yet. <a routerLink="/events">Browse events</a></p>
        </mat-card-content>
      </mat-card>
    } @else {
      <div class="bookings-list">
        @for (b of bookings(); track b.id) {
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ b.eventName }}</mat-card-title>
              <mat-card-subtitle>{{ b.referenceCode }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>{{ b.ticketCount }} ticket(s) · {{ b.totalAmount | currency }}</p>
              <p>Booked {{ b.createdAt | date:'medium' }}</p>
            </mat-card-content>
            <mat-card-actions>
              <a mat-button [routerLink]="['/bookings', b.id]">View Details</a>
              <mat-chip [color]="chipColor(b.status)">
                {{ statusLabel(b.status) }}
              </mat-chip>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    }
  `,
  styles: [`
    .bookings-list { display: flex; flex-direction: column; gap: 12px; }
    mat-card-actions { display: flex; align-items: center; gap: 12px; padding: 8px 16px; }
  `]
})
export class MyBookingsComponent implements OnInit {
  private readonly bookingsService = inject(BookingsService);
  readonly authService = inject(AuthService);
  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    // Phase 2: no hardcoded customer ID — API resolves customer from JWT
    this.bookingsService.getMyBookings().subscribe({
      next: data => { this.bookings.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  statusLabel(status: BookingStatus): string {
    return {
      [BookingStatus.Pending]: 'Pending',
      [BookingStatus.Confirmed]: 'Confirmed',
      [BookingStatus.Cancelled]: 'Cancelled',
      [BookingStatus.Refunded]: 'Refunded',
      [BookingStatus.Expired]: 'Expired'
    }[status];
  }

  chipColor(status: BookingStatus): string {
    return status === BookingStatus.Confirmed ? 'primary'
      : status === BookingStatus.Pending ? 'accent'
      : 'warn';
  }
}
