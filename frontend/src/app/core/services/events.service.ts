import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EventSummary, EventDetails, EventCategory } from '../models';

export interface EventsFilter {
  city?: string;
  category?: EventCategory;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class EventsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/events`;

  list(filter: EventsFilter = {}): Observable<EventSummary[]> {
    let params = new HttpParams();
    if (filter.city) params = params.set('city', filter.city);
    if (filter.category !== undefined) params = params.set('category', filter.category);
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.page) params = params.set('page', filter.page);
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize);
    return this.http.get<EventSummary[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<EventDetails> {
    return this.http.get<EventDetails>(`${this.baseUrl}/${id}`);
  }
}
