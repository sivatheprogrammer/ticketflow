using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Venues.AnyAsync()) return; // Already seeded

        // --- Venues ---
        var houstonArena   = new Venue("Toyota Center", "1510 Polk St", "Houston", 18055);
        var dallasArena    = new Venue("American Airlines Center", "2500 Victory Ave", "Dallas", 20000);
        var austinVenue    = new Venue("Moody Center", "2001 Robert Dedman Dr", "Austin", 15000);

        db.Venues.AddRange(houstonArena, dallasArena, austinVenue);
        await db.SaveChangesAsync();

        // --- Events ---
        var concertHouston = new Event(
            "Taylor Swift — The Eras Tour (Encore)",
            "The record-breaking Eras Tour makes its final Texas stop in Houston.",
            DateTime.UtcNow.AddDays(14),
            DateTime.UtcNow.AddDays(14).AddHours(3),
            EventCategory.Concert,
            houstonArena.Id);

        concertHouston.AddTicket(TicketTier.General, 89.99m, 500);
        concertHouston.AddTicket(TicketTier.Premium, 149.99m, 200);
        concertHouston.AddTicket(TicketTier.VIP, 299.99m, 50);
        concertHouston.Publish();

        var techConf = new Event(
            "Momentum Developer Conference 2026",
            "Texas's largest software engineering conference. Three days of talks, workshops, and networking.",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            EventCategory.Conference,
            austinVenue.Id);

        techConf.AddTicket(TicketTier.General, 299.00m, 800);
        techConf.AddTicket(TicketTier.VIP, 599.00m, 100);
        techConf.Publish();

        var sportsEvent = new Event(
            "Dallas Mavericks vs LA Lakers",
            "Western Conference matchup. Regular season game.",
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow.AddDays(7).AddHours(2).AddMinutes(30),
            EventCategory.Sports,
            dallasArena.Id);

        sportsEvent.AddTicket(TicketTier.General, 45.00m, 1000);
        sportsEvent.AddTicket(TicketTier.Premium, 120.00m, 400);
        sportsEvent.AddTicket(TicketTier.VIP, 350.00m, 80);
        sportsEvent.Publish();

        var draftEvent = new Event(
            "Houston Comedy Fest — Headliner Night",
            "Annual comedy festival headliner evening featuring top stand-up comics.",
            DateTime.UtcNow.AddDays(21),
            DateTime.UtcNow.AddDays(21).AddHours(3),
            EventCategory.Comedy,
            houstonArena.Id);

        draftEvent.AddTicket(TicketTier.General, 55.00m, 300);
        draftEvent.AddTicket(TicketTier.Premium, 95.00m, 100);
        // deliberately not publishing — demonstrates Draft state

        db.Events.AddRange(concertHouston, techConf, sportsEvent, draftEvent);

        // --- Customers ---
        var customer1 = new Customer("Alex Rivera", "alex.rivera@example.com", "+1-713-555-0101");
        var customer2 = new Customer("Jordan Lee", "jordan.lee@example.com");
        db.Customers.AddRange(customer1, customer2);

        await db.SaveChangesAsync();
    }
}
