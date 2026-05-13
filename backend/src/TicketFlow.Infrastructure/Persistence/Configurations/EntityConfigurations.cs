using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(200);
        b.Property(e => e.Description).HasMaxLength(2000);
        b.Property(e => e.Category).HasConversion<string>();
        b.Property(e => e.Status).HasConversion<string>();

        b.HasOne(e => e.Venue)
         .WithMany()
         .HasForeignKey(e => e.VenueId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(e => e.Tickets)
         .WithOne()
         .HasForeignKey(t => t.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(e => e.StartsAt);
        b.HasIndex(e => e.Status);
    }
}

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Name).IsRequired().HasMaxLength(200);
        b.Property(v => v.Address).HasMaxLength(500);
        b.Property(v => v.City).IsRequired().HasMaxLength(100);
        b.HasIndex(v => v.City);
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.ReferenceCode).IsRequired().HasMaxLength(20);
        b.Property(t => t.Price).HasPrecision(18, 2);
        b.Property(t => t.Tier).HasConversion<string>();
        b.Property(t => t.Status).HasConversion<string>();
        b.HasIndex(t => t.ReferenceCode).IsUnique();
        b.HasIndex(t => t.Status);
        b.HasIndex(t => t.ReservedUntil);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.HasKey(bk => bk.Id);
        b.Property(bk => bk.ReferenceCode).IsRequired().HasMaxLength(20);
        b.Property(bk => bk.TotalAmount).HasPrecision(18, 2);
        b.Property(bk => bk.Status).HasConversion<string>();

        b.HasMany(bk => bk.Tickets)
         .WithOne()
         .HasForeignKey(t => t.BookingId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(bk => bk.ReferenceCode).IsUnique();
        b.HasIndex(bk => bk.CustomerId);
        b.HasIndex(bk => bk.Status);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        b.Property(c => c.Email).IsRequired().HasMaxLength(256);
        b.Property(c => c.PhoneNumber).HasMaxLength(20);
        b.HasIndex(c => c.Email).IsUnique();
    }
}
