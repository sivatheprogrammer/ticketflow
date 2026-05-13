namespace TicketFlow.Domain.Enums;

public enum EventStatus
{
    Draft = 0,
    Published = 1,
    Cancelled = 2,
    Completed = 3
}

public enum EventCategory
{
    Concert = 0,
    Sports = 1,
    Conference = 2,
    Theater = 3,
    Comedy = 4,
    Other = 99
}

public enum TicketTier
{
    General = 0,
    Premium = 1,
    VIP = 2
}

public enum TicketStatus
{
    Available = 0,
    Reserved = 1,
    Booked = 2,
    Used = 3,
    Cancelled = 4
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Refunded = 3,
    Expired = 4
}
