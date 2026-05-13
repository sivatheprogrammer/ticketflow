import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { EventsService } from '../../../core/services/events.service';
import { EventSummary } from '../../../core/models';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatChipsModule
  ],
  template: `
    <h1>Upcoming Events</h1>

    @if (loading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (events().length === 0) {
      <p>No events found.</p>
    } @else {
      <div class="event-grid">
        @for (event of events(); track event.id) {
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ event.name }}</mat-card-title>
              <mat-card-subtitle>
                {{ event.venueName }} · {{ event.city }}
              </mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>{{ event.startsAt | date:'medium' }}</p>
              <p><strong>From {{ event.minPrice | currency }}</strong></p>
            </mat-card-content>
            <mat-card-actions>
              <a mat-button color="primary" [routerLink]="['/events', event.id]">
                View Details
              </a>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    }
  `,
  styles: [`
    .event-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
      margin-top: 1rem;
    }
  `]
})
export class EventListComponent implements OnInit {
  private readonly eventsService = inject(EventsService);

  readonly events = signal<EventSummary[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.eventsService.list().subscribe({
      next: data => {
        this.events.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
