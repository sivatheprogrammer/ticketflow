import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { BookingsService } from '../../core/services/bookings.service';
import { Booking, BookingStatus } from '../../core/models';

// Phase 1: hardcoded demo customer. Replaced by JWT claim in Phase 2.
const DEMO_CUSTOMER_ID = '00000000-0000-0000-0000-000000000001';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatChipsModule
  ],
  template: `
    <h1 style="margin-bottom:16px">My Bookings</h1>

    @if (loading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (bookings().length === 0) {
      <mat-card>
        <mat-card-content>
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
              <p>{{ b.createdAt | date:'medium' }}</p>
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
  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.bookingsService.listForCustomer(DEMO_CUSTOMER_ID).subscribe({
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
