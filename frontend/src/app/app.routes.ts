import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  {
    path: 'events',
    loadComponent: () =>
      import('./features/events/event-list/event-list.component')
        .then(m => m.EventListComponent)
  },
  {
    path: 'events/:id',
    loadComponent: () =>
      import('./features/events/event-details/event-details.component')
        .then(m => m.EventDetailsComponent)
  },
  {
    path: 'bookings/:id',
    canActivate: [authGuard], // Protected — must be logged in
    loadComponent: () =>
      import('./features/bookings/booking-detail/booking-detail.component')
        .then(m => m.BookingDetailComponent)
  },
  {
    path: 'my-bookings',
    canActivate: [authGuard], // Protected — must be logged in
    loadComponent: () =>
      import('./features/my-bookings/my-bookings.component')
        .then(m => m.MyBookingsComponent)
  },
  { path: '**', redirectTo: 'events' }
];
