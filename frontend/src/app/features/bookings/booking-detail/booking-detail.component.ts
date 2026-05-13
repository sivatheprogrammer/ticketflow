import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { BookingsService } from '../../../core/services/bookings.service';
import { Booking, BookingStatus } from '../../../core/models';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule,
    MatProgressSpinnerModule, MatChipsModule, MatDividerModule
  ],
  template: `
    @if (loading()) {
      <div style="display:flex;justify-content:center;padding:48px">
        <mat-spinner diameter="48"></mat-spinner>
      </div>
    } @else if (booking()) {
      <mat-card style="margin-top:16px">
        <mat-card-header>
          <mat-card-title>Booking {{ booking()!.referenceCode }}</mat-card-title>
          <mat-card-subtitle>{{ booking()!.eventName }}</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <p><strong>Status:</strong> {{ statusLabel(booking()!.status) }}</p>
          <p><strong>Total:</strong> {{ booking()!.totalAmount | currency }}</p>
          <p><strong>Tickets:</strong> {{ booking()!.ticketCount }}</p>

          @if (isPending() && countdown() > 0) {
            <div class="countdown-banner">
              ⏱ Reservation expires in <strong>{{ countdownLabel() }}</strong> — confirm now to secure your tickets.
            </div>
          }
        </mat-card-content>

        <mat-card-actions>
          @if (isPending()) {
            <button mat-raised-button color="primary"
              [disabled]="confirming()"
              (click)="confirm()">
              {{ confirming() ? 'Confirming...' : 'Confirm & Pay' }}
            </button>
            <button mat-button color="warn" (click)="cancel()">
              Cancel Booking
            </button>
          }
          @if (isConfirmed()) {
            <mat-chip color="primary">✓ Confirmed — Your tickets are secured</mat-chip>
          }
        </mat-card-actions>
      </mat-card>
    }
  `,
  styles: [`
    .countdown-banner {
      background: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 4px;
      padding: 12px 16px;
      margin: 16px 0;
      color: #856404;
    }
    mat-card-actions { padding: 8px 16px; display: flex; gap: 8px; }
  `]
})
export class BookingDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingsService = inject(BookingsService);

  readonly booking = signal<Booking | null>(null);
  readonly loading = signal(true);
  readonly confirming = signal(false);
  readonly countdown = signal(0);

  private timer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.bookingsService.getById(id).subscribe({
      next: data => {
        this.booking.set(data);
        this.loading.set(false);
        if (data.status === BookingStatus.Pending && data.reservedUntil) {
          this.startCountdown(new Date(data.reservedUntil));
        }
      },
      error: () => this.loading.set(false)
    });
  }

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
  }

  private startCountdown(until: Date): void {
    this.timer = setInterval(() => {
      const remaining = Math.max(0, until.getTime() - Date.now());
      this.countdown.set(remaining);
      if (remaining === 0) clearInterval(this.timer);
    }, 1000);
  }

  countdownLabel(): string {
    const ms = this.countdown();
    const m = Math.floor(ms / 60000);
    const s = Math.floor((ms % 60000) / 1000);
    return `${m}:${s.toString().padStart(2, '0')}`;
  }

  isPending(): boolean { return this.booking()?.status === BookingStatus.Pending; }
  isConfirmed(): boolean { return this.booking()?.status === BookingStatus.Confirmed; }

  statusLabel(status: BookingStatus): string {
    return {
      [BookingStatus.Pending]: 'Pending — awaiting confirmation',
      [BookingStatus.Confirmed]: 'Confirmed',
      [BookingStatus.Cancelled]: 'Cancelled',
      [BookingStatus.Refunded]: 'Refunded',
      [BookingStatus.Expired]: 'Expired'
    }[status];
  }

  confirm(): void {
    this.confirming.set(true);
    this.bookingsService.confirm(this.booking()!.id).subscribe({
      next: () => {
        this.booking.update(b => b ? { ...b, status: BookingStatus.Confirmed } : b);
        this.confirming.set(false);
        if (this.timer) clearInterval(this.timer);
      },
      error: () => this.confirming.set(false)
    });
  }

  cancel(): void {
    this.bookingsService.cancel(this.booking()!.id).subscribe({
      next: () => this.router.navigate(['/my-bookings'])
    });
  }
}
