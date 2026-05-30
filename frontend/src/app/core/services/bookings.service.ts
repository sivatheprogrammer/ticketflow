import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Booking, CreateBookingResponse, TicketTier } from '../models';

// Phase 2: CustomerId removed — API extracts it from JWT token
export interface CreateBookingRequest {
  eventId: string;
  quantity: number;
  tier: TicketTier;
}

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/bookings`;

  create(request: CreateBookingRequest): Observable<CreateBookingResponse> {
    return this.http.post<CreateBookingResponse>(this.baseUrl, request);
  }

  confirm(bookingId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${bookingId}/confirm`, {});
  }

  cancel(bookingId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${bookingId}/cancel`, {});
  }

  getById(bookingId: string): Observable<Booking> {
    return this.http.get<Booking>(`${this.baseUrl}/${bookingId}`);
  }

  // Phase 2: Uses /my endpoint which extracts customer from JWT
  getMyBookings(): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.baseUrl}/my`);
  }
}
