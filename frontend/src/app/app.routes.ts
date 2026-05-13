import { Routes } from '@angular/router';

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
    loadComponent: () =>
      import('./features/bookings/booking-detail/booking-detail.component')
        .then(m => m.BookingDetailComponent)
  },
  {
    path: 'my-bookings',
    loadComponent: () =>
      import('./features/my-bookings/my-bookings.component')
        .then(m => m.MyBookingsComponent)
  },
  { path: '**', redirectTo: 'events' }
];
