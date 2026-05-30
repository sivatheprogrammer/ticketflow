import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EventsService } from '../../../core/services/events.service';
import { BookingsService } from '../../../core/services/bookings.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventDetails, TicketTier } from '../../../core/models';

@Component({
  selector: 'app-event-details',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatButtonModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  template: `
    @if (loading()) {
      <div style="display:flex;justify-content:center;padding:48px">
        <mat-spinner diameter="48"></mat-spinner>
      </div>
    } @else if (event()) {
      <mat-card style="margin-top:16px">
        <mat-card-header>
          <mat-card-title>{{ event()!.name }}</mat-card-title>
          <mat-card-subtitle>
            {{ event()!.venueName }} · {{ event()!.city }} ·
            {{ event()!.startsAt | date:'fullDate' }}
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <p style="margin: 16px 0">{{ event()!.description }}</p>
          <mat-divider />

          <h3 style="margin: 16px 0 8px">Available Tickets</h3>

          @for (tier of event()!.ticketTiers; track tier.tier) {
            <div class="tier-row">
              <div>
                <strong>{{ tierLabel(tier.tier) }}</strong>
                <span class="tier-count">{{ tier.availableCount }} remaining</span>
              </div>
              <div class="tier-right">
                <span class="tier-price">{{ tier.price | currency }}</span>

                @if (authService.isAuthenticated()) {
                  <button mat-raised-button color="primary"
                    [disabled]="tier.availableCount === 0 || reserving()"
                    (click)="reserve(tier.tier)">
                    {{ reserving() ? 'Reserving...' : 'Reserve' }}
                  </button>
                } @else {
                  <button mat-stroked-button (click)="authService.login()">
                    Sign in to Reserve
                  </button>
                }
              </div>
            </div>
            <mat-divider />
          }
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [`
    mat-card { margin-top: 16px; }
    .tier-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 0;
    }
    .tier-count { color: #666; margin-left: 12px; font-size: 0.875rem; }
    .tier-right { display: flex; align-items: center; gap: 16px; }
    .tier-price { font-size: 1.1rem; font-weight: 500; }
  `]
})
export class EventDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly eventsService = inject(EventsService);
  private readonly bookingsService = inject(BookingsService);
  private readonly snack = inject(MatSnackBar);

  // Phase 2: AuthService injected — no more DEMO_CUSTOMER_ID
  readonly authService = inject(AuthService);

  readonly event = signal<EventDetails | null>(null);
  readonly loading = signal(true);
  readonly reserving = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.eventsService.getById(id).subscribe({
      next: data => { this.event.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  tierLabel(tier: TicketTier): string {
    return {
      [TicketTier.General]: 'General',
      [TicketTier.Premium]: 'Premium',
      [TicketTier.VIP]: 'VIP'
    }[tier];
  }

  reserve(tier: TicketTier): void {
    this.reserving.set(true);

    // Phase 2: No customerId in the request body — the API extracts it from the JWT token
    this.bookingsService.create({
      eventId: this.event()!.id,
      quantity: 1,
      tier
    }).subscribe({
      next: result => {
        this.snack.open(
          `Booking ${result.referenceCode} reserved! Confirm within 15 minutes.`,
          'OK', { duration: 6000 });
        this.router.navigate(['/bookings', result.bookingId]);
      },
      error: () => this.reserving.set(false)
    });
  }
}
