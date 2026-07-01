using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Messages;

namespace TicketFlow.Api.Infrastructure;

public class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _bookingCreatedSender;
    private readonly ServiceBusSender _bookingConfirmedSender;

    public ServiceBusEventPublisher(IConfiguration configuration)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"]!;
        _client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        });
        _bookingCreatedSender = _client.CreateSender(
            configuration["ServiceBus:BookingCreatedQueue"] ?? "booking-created");
        _bookingConfirmedSender = _client.CreateSender(
            configuration["ServiceBus:BookingConfirmedQueue"] ?? "booking-confirmed");
    }

    public async Task PublishBookingCreatedAsync(BookingCreatedEvent @event, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "BookingCreated"
        };
        await _bookingCreatedSender.SendMessageAsync(message, ct);
    }

    public async Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "BookingConfirmed"
        };
        await _bookingConfirmedSender.SendMessageAsync(message, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _bookingCreatedSender.DisposeAsync();
        await _bookingConfirmedSender.DisposeAsync();
        await _client.DisposeAsync();
    }
}