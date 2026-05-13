export enum EventStatus {
  Draft = 0,
  Published = 1,
  Cancelled = 2,
  Completed = 3
}

export enum EventCategory {
  Concert = 0,
  Sports = 1,
  Conference = 2,
  Theater = 3,
  Comedy = 4,
  Other = 99
}

export enum TicketTier {
  General = 0,
  Premium = 1,
  VIP = 2
}

export enum TicketStatus {
  Available = 0,
  Reserved = 1,
  Booked = 2,
  Used = 3,
  Cancelled = 4
}

export enum BookingStatus {
  Pending = 0,
  Confirmed = 1,
  Cancelled = 2,
  Refunded = 3,
  Expired = 4
}

export interface Venue {
  id: string;
  name: string;
  address: string;
  city: string;
  capacity: number;
}

export interface EventSummary {
  id: string;
  name: string;
  startsAt: string;
  category: EventCategory;
  venueName: string;
  city: string;
  minPrice: number;
}

export interface EventDetails extends EventSummary {
  description: string;
  endsAt: string;
  status: EventStatus;
  ticketTiers: TicketTierAvailability[];
}

export interface TicketTierAvailability {
  tier: TicketTier;
  price: number;
  availableCount: number;
}

export interface Booking {
  id: string;
  referenceCode: string;
  eventId: string;
  eventName: string;
  status: BookingStatus;
  totalAmount: number;
  ticketCount: number;
  createdAt: string;
  reservedUntil?: string;
}

export interface CreateBookingRequest {
  customerId: string;
  eventId: string;
  quantity: number;
  tier: TicketTier;
}

export interface CreateBookingResponse {
  bookingId: string;
  referenceCode: string;
  totalAmount: number;
}
